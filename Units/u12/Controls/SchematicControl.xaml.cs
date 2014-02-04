using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Timers;
using Prism.General;
using Prism.Controls;
using Prism.Units.Classes;
using Prism.General.Automation;

namespace Prism.Units.Controls
{
    /// <summary>
    /// Логика взаимодействия для SchematicControl.xaml
    /// </summary>
    public partial class SchematicControl : UserControl
    {
        private class SchematicItem
        {
            public string View { get; set; }
            public string Number { get; set; }

            public SchematicItem(string view, string number)
            {
                View = view;
                Number = number;
            }
        }

        private Unit Unit;
        private ShockwaveFlashControl SchematicFlashControl;
        private Queue<SchematicItem> UpdateQueue;

        public SchematicControl(Unit unit, String title)
        {
            UpdateQueue = new Queue<SchematicItem>();
            
            InitializeComponent();

            this.Unit = unit;
            this.titleText.Text = title;

            SchematicFlashControl = new ShockwaveFlashControl(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Unit\\u11\\Schematic.swf");
            SchematicFlashControl.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                Schematic_Initialize();
                Schematic_SetObjectParam("view", "avaliable", "number", "1");
                Schematic_SetObjectParam("view", "avaliable", "number", Unit.Processing.IsOnline ? "0" : "1");
                
                Unit.Processing.ProcessingOnlineStateChangedEvent += OnlineStateChanged;
                Unit.Processing.ProcessingParamUpdateEvent += ParamValueChanged;

                UpdateAll();
                //IsLoaded = true;
            };
            mainGrid.Children.Add(SchematicFlashControl);
        }

        ~SchematicControl()
        {
            Unit.Processing.ProcessingOnlineStateChangedEvent -= OnlineStateChanged;
            Unit.Processing.ProcessingParamUpdateEvent -= ParamValueChanged;
        }

        private void Schematic_Initialize()
        {
            try
            {
                SchematicFlashControl.CallFunction("<invoke name=\"Initialize\"></invoke>");
            }
            catch (SystemException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void Schematic_SetObjectParam(string obj, string param, string type, string value)
        {
            try
            {
                SchematicFlashControl.CallFunction("<invoke name=\"SetObjectParam\"><arguments><string>" + obj + "</string><string>" + param + "</string><" + type + ">" + value + "</" + type + "></arguments></invoke>");
            }
            catch (SystemException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            
        }

        public void OnlineStateChanged(object sender, bool isOnline)
        {
            MainThread.EnqueueTask(delegate()
            {
                Schematic_SetObjectParam("view", "avaliable", "number", isOnline ? "0" : "1");
            });
        }

        public void ParamValueChanged(object sender, Param param)
        {
            UpdateParam(param);
            ProcessUpdateQueue();
        }

        public void UpdateAll()
        {
            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                foreach (KeyValuePair<string, Param> keyValue in Unit.Processing.Params)
                {
                    UpdateParam(keyValue.Value);
                }
                ProcessUpdateQueue();
            });
        }

        public void UpdateParam(Param param)
        {
            if (param.Type == Param.ParamAssignType.Value)
            {
                if (param.Value != null)
                {
                    UpdateQueue.Enqueue(new SchematicItem(param.Name, param.Value));
                }
            }
            else
            {
                UpdateQueue.Enqueue(new SchematicItem(param.Name, ((uint)param.State).ToString()));
            }
        }

        public void ProcessUpdateQueue()
        {
            if (UpdateQueue.Count > 0)
            {
                MainThread.EnqueueTask(delegate()
                {
                    //try
                    //{
                        SchematicItem item = UpdateQueue.Dequeue();
                        Schematic_SetObjectParam("view", item.View, "number", item.Number);                        
                    //}
                    //catch (Exception e)
                    //{
                    //}

                    ProcessUpdateQueue();
                });
            }
        }
    }
}

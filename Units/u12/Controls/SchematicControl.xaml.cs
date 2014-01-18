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
        private Unit Unit;
        private ShockwaveFlashControl schematicControl;
        private bool IsLoaded = false;

        public SchematicControl(Unit unit, String title)
        {
            InitializeComponent();

            this.Unit = unit;
            this.titleText.Text = title;

            schematicControl = new ShockwaveFlashControl(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Unit\\u12\\Schematic.swf");
            schematicControl.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                Schematic_Initialize();
                Schematic_SetObjectParam("view", "avaliable", "number", "1");
                unit.Processing.ProcessingUpdateEvent += Update;
                
                Update(null);
                Schematic_SetObjectParam("view", "avaliable", "number", Unit.Processing.IsAvaliable ? "0" : "1");

                Unit.Processing.ProcessingChangeStateEvent += delegate(object s)
                {
                    MainThread.EnqueueTask(delegate()
                    {
                        Schematic_SetObjectParam("view", "avaliable", "number", Unit.Processing.IsAvaliable ? "0" : "1");
                    });
                };

                IsLoaded = true;
            };
            mainGrid.Children.Add(schematicControl);
        }

        ~SchematicControl()
        {
            Unit.Processing.ProcessingUpdateEvent -= Update;
        }

        private void Schematic_Initialize()
        {
            try
            {
                schematicControl.CallFunction("<invoke name=\"Initialize\"></invoke>");
            }
            catch (SystemException e)
            {
            }
        }

        private void Schematic_SetObjectParam(string obj, string param, string type, string value)
        {
            try
            {
                schematicControl.CallFunction("<invoke name=\"SetObjectParam\"><arguments><string>" + obj + "</string><string>" + param + "</string><" + type + ">" + value + "</" + type + "></arguments></invoke>");
            }
            catch (SystemException e)
            {
            }
        }

        public void Update(object sender)
        {
            MainThread.EnqueueTask(delegate()
            {
                foreach (KeyValuePair<string, Param> keyValue in Unit.Processing.Params)
                {
                    Param param = keyValue.Value;

                    if (param.Type == Param.ParamAssignType.Value)
                    {
                        string value = param.Value;
                        
                        if (value != null)
                        {
                            Schematic_SetObjectParam("view", keyValue.Key, "number", value);
                        }
                    }
                    else
                    {
                        uint value = (uint)param.State;
                        Schematic_SetObjectParam("view", keyValue.Key, "number", value.ToString());
                    }
                }                
            });
        }

        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Schematic_SetObjectParam("view", "avaliable", "number", Unit.Processing.IsAvaliable ? "0" : "1");
            }            
        }
    }
}

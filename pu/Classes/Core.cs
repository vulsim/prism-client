using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Windows;
using Prism.General;
using Prism.Properties;
using Prism.General.Automation;

namespace Prism.Classes
{
    public delegate void CoreBusyStateChangedEventHandler(object sender, bool isBusy);
    public delegate void CoreGeneralStateChangedEventHandler(object sender, ParamState state);
    
    public class Core
    {        
        public static Core Instance = null;

        public List<Unit> Units;
        public Journal Journal;
        public ParamState CoreGeneralState;
        public bool IsCoreBusy;

        public event CoreBusyStateChangedEventHandler CoreBusyStateChangedEvent;
        public event CoreGeneralStateChangedEventHandler CoreGeneralStateChangedEvent;

        private Prism.Windows.SplashScreen SplashScreen;

        public Core()
        {            
            ThreadPool.QueueUserWorkItem(delegate(object target) 
            {
                Startup(delegate()
                {
 
                });
            }, null);
        }

        public void Startup(SimpleCompleteHandler completion)
        {            
            MainThread.EnqueueTask(delegate()
            {
                SplashScreen = new Prism.Windows.SplashScreen();
                SplashScreen.Show();
            }); 
            
            ContentLoader contentLoader = new ContentLoader(delegate(uint stage, string description)
            {
                MainThread.EnqueueTask(delegate()
                {
                    SplashScreen.ProgressLabel.Content = description;                    
                });            
            });

            contentLoader.Enqueue(Resources.CORE_STARTUP_MESSAGE_0, delegate(object target)
            {
                Units = new List<Unit>();
                Journal = new Journal();
                AlarmNotificationCenter.Instance = new AlarmNotificationCenter();
            }, null);

            contentLoader.Enqueue(Resources.CORE_STARTUP_MESSAGE_1, delegate(object target)
            {
                try 
                {
                    AssemblySettings assemblySettings = AssemblySettings.Load("assembly.conf");

                    foreach (UnitSettings settings in assemblySettings.Units)
                    {
                        contentLoader.Enqueue(String.Format(Resources.CORE_STARTUP_MESSAGE_2, settings.FullName), delegate(object target1)
                        {
                            UnitSettings unitSettings = (UnitSettings)target1;

                            try
                            {
                                ManualResetEvent ContinueEvent = new ManualResetEvent(false);
                                Unit unit = new Unit(unitSettings, this.Journal, delegate()
                                {
                                    ContinueEvent.Set();
                                });
                                ContinueEvent.WaitOne();
                                Units.Add(unit);

                                unit.UnitBusyStateChangedEvent += delegate(object sender, bool isBusy)
                                {
                                    CoreUpdateBusyState();
                                };

                                unit.UnitStateChangedEvent += delegate(object sender, ParamState state)
                                {
                                    CoreUpdateGeneralState();
                                };

                                unit.UnitAlarmsChangedEvent += delegate(object sender, IEnumerable<IAlarm> alarms)
                                {
                                    AlarmNotificationCenter.Instance.Update((Unit)sender);
                                };

                                unit.Uri = new Uri("unit://" + Units.Count.ToString());
                            }
                            catch (SystemException e)
                            {
                            }
                        }, settings);  
                    }

                    contentLoader.Enqueue(Resources.CORE_STARTUP_MESSAGE_3, delegate(object target2)
                    {
                        CoreUpdateGeneralState();

                        MainThread.EnqueueTask(delegate()
                        {
                            MainWindow.Instance = new MainWindow();
                            MainWindow.Instance.Show();
                            SplashScreen.Close();
                            SplashScreen = null;

                            CoreUpdateBusyState();
                        });                                                
                    }, null);
                }
                catch (SystemException e)
                {
                }                
            }, null);           
        }

        public void Shutdown()
        {
            ThreadPool.QueueUserWorkItem(delegate(object target) 
            {
                System.Environment.Exit(0);
            }, null);            
        }

        private void CoreUpdateGeneralState()
        {
            ParamState newState = ParamState.Unknown;

            foreach (Unit unit in Units)
            {
                if (unit.State > newState)
                {
                    newState = unit.State;
                }
            }

            if (CoreGeneralState != newState) 
            {
                CoreGeneralState = newState;

                if (CoreGeneralStateChangedEvent != null)
                {
                    CoreGeneralStateChangedEvent(this, CoreGeneralState);
                }
            }
        }

        private void CoreUpdateBusyState()
        {
            bool isBusy = false;

            foreach (Unit unit in Units)
            {
                if (unit.IsBusy)
                {
                    isBusy = true;
                    break;
                }
            }

            if (IsCoreBusy != isBusy)
            {
                IsCoreBusy = isBusy;

                if (CoreBusyStateChangedEvent != null)
                {
                    CoreBusyStateChangedEvent(this, IsCoreBusy);
                }
            }
        }
    }
}

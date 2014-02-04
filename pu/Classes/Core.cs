using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
        public PollManager PollManager;
        public ParamState CoreGeneralState;
        public bool IsBusy;

        public event CoreBusyStateChangedEventHandler CoreBusyStateChangedEvent;
        public event CoreGeneralStateChangedEventHandler CoreGeneralStateChangedEvent;

        private Prism.Windows.SplashScreen SplashScreen;

        public Core()
        {
            Startup(delegate()
            {

            });
        }

        public void Startup(SimpleCompleteHandler completion)
        {            
            SplashScreen = new Prism.Windows.SplashScreen();            
            SplashScreen.Show();

            ContentLoader contentLoader = new ContentLoader(delegate(double progressValue, string description)
            {
                MainThread.EnqueueTask(delegate()
                {                    
                    SplashScreen.progressLabel.Content = description;                    
                });
            });

            contentLoader.Enqueue(Resources.CORE_STARTUP_MESSAGE_0, delegate(object target)
            {
                Units = new List<Unit>();
                Journal = new Journal();
                PollManager = new PollManager();
                PollManager.PollManagerChangeStateEvent += CoreUpdateBusyState;
                PollManager.PollManagerUpdateUnitStateEvent += CoreUpdateUnitStateEvent;

                UpdateManager.Instance = new UpdateManager();
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
                                Unit unit = new Unit(unitSettings, this.PollManager, this.Journal, delegate()
                                {
                                    ContinueEvent.Set();
                                });
                                ContinueEvent.WaitOne();
                                Units.Add(unit);

                                unit.Uri = new Uri("unit://" + Units.Count.ToString());

                                unit.UnitStateChangedEvent += delegate(object sender, ParamState state)
                                {
                                    CoreUpdateGeneralState();
                                };

                                unit.UnitAlarmUpdateEvent += delegate(object sender, IAlarm alarm)
                                {
                                    AlarmNotificationCenter.Instance.UpdateAlarm((Unit)sender, alarm);
                                };
                            }
                            catch (SystemException e)
                            {
                                System.Diagnostics.Debug.WriteLine(e.ToString());
                            }
                        }, settings);  
                    }

                    contentLoader.Enqueue(Resources.CORE_STARTUP_MESSAGE_3, delegate(object target2)
                    {
                        MainThread.EnqueueTask(delegate()
                        {
                            MainWindow.Instance = new MainWindow();
                            MainWindow.Instance.Show();
                            SplashScreen.Close();
                            SplashScreen = null;

                            CoreUpdateGeneralState();
                            PollManager.Start();
                        });
                        completion();  
                    }, null);
                }
                catch (SystemException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }                
            }, null);           
        }

        public void Shutdown()
        {
            PollManager.PollManagerChangeStateEvent -= CoreUpdateBusyState;
            PollManager.PollManagerUpdateUnitStateEvent -= CoreUpdateUnitStateEvent;

            MainThread.EnqueueTask(delegate() 
            {
                try
                {
                    System.Environment.Exit(0);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }                
            });            
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
                    MainThread.EnqueueTask(delegate()
                    {
                        CoreGeneralStateChangedEvent(this, CoreGeneralState);
                    });                    
                }
            }
        }

        private void CoreUpdateBusyState(bool isBusy)
        {
            if (IsBusy != isBusy)
            {
                IsBusy = isBusy;

                if (CoreBusyStateChangedEvent != null)
                {
                    CoreBusyStateChangedEvent(this, IsBusy);
                }
            }
        }

        private void CoreUpdateUnitStateEvent(IUnit unit)
        {
            CoreUpdateGeneralState();
        }
    }
}

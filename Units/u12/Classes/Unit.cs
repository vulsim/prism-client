using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Timers;
using Prism.General;
using Prism.General.Automation;

namespace Prism.Units.Classes
{
    public class Unit : IUnit
    {
        public IJournal Journal;
        public IPollManager Manager;
        public UnitSettings Settings;
        public Processing Processing;

        public bool IsOnline { get { return Processing.IsOnline; } }        
        public ParamState State { get { return LastState; } }

        public string FullName { get { return Settings.FullName; } }
        public string ShortName { get { return Settings.ShortName; } }
        public string SymbolicName { get { return Settings.SymbolicName; } }
        public string Address { get { return Settings.Address; } }

        public IEnumerable<IAlarm> Alarms { get { return AlarmsInternal; } }
        public IEnumerable<IPresentationControl> PresentationControls { get { return PresentationControlsInternal; } }

        public event UnitParamUpdateEventHandler UnitParamUpdateEvent;
        public event UnitAlarmUpdateEventHandler UnitAlarmUpdateEvent;
        public event UnitStateChangedEventHandler UnitStateChangedEvent;

        private ParamState LastState = ParamState.Unknown;
        private List<Alarm> AlarmsInternal;
        private List<PresentationControl> PresentationControlsInternal;

        public Unit()
        {
            AlarmsInternal = new List<Alarm>();
            PresentationControlsInternal = new List<PresentationControl>();
        }

        public void Initialize(Dispatcher dispatcher, UnitSettings settings, IPollManager manager, IJournal journal, UnitGeneralCompleteHandler complete)
        {
            MainThread.Initialize(dispatcher);

            Settings = settings;
            Journal = journal;
            Manager = manager;

            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                Processing = new Processing(this);
                Processing.ProcessingAlarmUpdateEvent += AlarmUpdateEvent;
                Processing.ProcessingParamUpdateEvent += ParamUpdateEvent;

                ManualResetEvent ContinueEvent = new ManualResetEvent(false);
                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category0, "Схема подстанции", "Общий контроль состояния", new Prism.Units.Controls.SchematicControl(this, "схема подстанции")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №1", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 1, "линейный автомат №1")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №2", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 2, "линейный автомат №2")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №3", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 3, "линейный автомат №3")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №4", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 4, "линейный автомат №4")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №5", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 5, "линейный автомат №5")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "АЗ", "Контроль и управление", new Prism.Units.Controls.Lsw9Control(this, "запасной автомат")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category2, "ВА №1", "Контроль и управление", new Prism.Units.Controls.RectControl(this, 1, "выпрямительный агрегат №1")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category2, "ВА №2", "Контроль и управление", new Prism.Units.Controls.RectControl(this, 2, "выпрямительный агрегат №2")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category2, "ВА №3", "Контроль и управление", new Prism.Units.Controls.RectControl(this, 3, "выпрямительный агрегат №3")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category1, "Рабочий ввод", "Контроль и управление", new Prism.Units.Controls.LeadinControl(this, 1, "рабочий ввод")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category1, "Резервный ввод", "Контроль и управление", new Prism.Units.Controls.LeadinControl(this, 2, "резервный ввод")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category1, "Отходящая линия", "Контроль и управление", new Prism.Units.Controls.LeadinControl(this, 3, "отходящая линия")));
                });

                MainThread.EnqueueTask(delegate()
                {
                    ContinueEvent.Set();
                });

                ContinueEvent.WaitOne();
                complete();
            }, null);
        }

        public void Uninitialize()
        {
            Processing.ProcessingAlarmUpdateEvent -= AlarmUpdateEvent;
            Processing.ProcessingParamUpdateEvent -= ParamUpdateEvent;
        }

        private void UpdateUnitState()
        {
            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                ParamState currentState = Processing.Params["common_state"].State;

                foreach (var alarmInternal in AlarmsInternal)
                {
                    if (alarmInternal.State > currentState)
                    {
                        currentState = alarmInternal.State;

                        if (currentState == ParamState.C)
                        {
                            break;
                        }
                    }
                }

                if (LastState != currentState)
                {
                    LastState = currentState;

                    if (UnitStateChangedEvent != null)
                    {
                        UnitStateChangedEvent(this, LastState);
                    }
                }
            }, null);            
        }

        private void ParamUpdateEvent(object sender, Param param)
        {
            if (param.Name.Equals("common_state"))
            {
                UpdateUnitState();
            }
            else if (UnitParamUpdateEvent != null)
            {
                UnitParamUpdateEvent(this, param);

            }
        }

        private void AlarmUpdateEvent(object sender, Alarm alarm)
        {
            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                Alarm newAlarm = (Alarm)target;
                bool IsNewAlarm = true;

                foreach (var alarmInternal in AlarmsInternal)
                {
                    if (alarmInternal.Code.Equals(alarm.Code))
                    {
                        IsNewAlarm = false;
                        if (alarmInternal.State != alarm.State)
                        {
                            if (alarm.State == ParamState.Idle)
                            {
                                AlarmsInternal.Remove(alarmInternal);
                            }
                            else
                            {
                                alarmInternal.State = alarm.State;
                            }

                            UpdateUnitState();
                        }

                        break;
                    }
                }

                if (IsNewAlarm)
                {
                    AlarmsInternal.Add(alarm);
                    UpdateUnitState();
                }

            }, alarm);
            
            if (UnitAlarmUpdateEvent != null)
            {
                UnitAlarmUpdateEvent(this, alarm);
            }
        }        
    }
}

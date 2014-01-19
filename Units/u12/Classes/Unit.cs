using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Prism.General;
using Prism.General.Automation;

namespace Prism.Units.Classes
{
    public class Unit : IUnit
    {
        public IJournal Journal;
        public UnitSettings Settings;
        public Processing Processing;

        public bool IsOnline { get { return Processing.IsAvaliable; } }        
        public bool IsBusy { get { return Processing.IsBusy; } }
        public ParamState State { get { return LastState; } }

        public string FullName { get { return Settings.FullName; } }
        public string ShortName { get { return Settings.ShortName; } }
        public string SymbolicName { get { return Settings.SymbolicName; } }
        public string Address { get { return Settings.Address; } }

        public IEnumerable<IAlarm> Alarms { get { return AlarmsInternal; } }
        public IEnumerable<IPresentationControl> PresentationControls { get { return PresentationControlsInternal; } }

        private bool LastIsBusy;
        private ParamState LastState = ParamState.B;
        private List<Alarm> AlarmsInternal;
        private List<PresentationControl> PresentationControlsInternal;

        public event UnitBusyStateChangedEventHandler UnitBusyStateChangedEvent;
        public event UnitStateChangedEventHandler UnitStateChangedEvent;
        public event UnitAlarmsChangedEventHandler UnitAlarmsChangedEvent;

        public Unit()
        {
            LastIsBusy = false;
            LastState = ParamState.Unknown;
            AlarmsInternal = new List<Alarm>();
            PresentationControlsInternal = new List<PresentationControl>();
        }

        public void Initialize(UnitSettings settings, IJournal journal, UnitGeneralCompleteHandler complete)
        {
            this.Settings = settings;
            this.Journal = journal;

            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                Processing = new Processing(Settings, this);
                Processing.ProcessingUpdateEvent += ProcessingUpdateEvent;
                Processing.ProcessingChangeStateEvent += ProcessingChangeStateEvent;

                ManualResetEvent ContinueEvent = new ManualResetEvent(false);
                MainThread.EnqueueTask(delegate()
                {
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category0, "Схема подстанции", "Общий контроль состояния", new Prism.Units.Controls.SchematicControl(this, "схема подстанции")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №1", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 1, "линейный автомат №1")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №2", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 2, "линейный автомат №2")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №3", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 3, "линейный автомат №3")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №4", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 4, "линейный автомат №4")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "ЛА №5", "Контроль и управление", new Prism.Units.Controls.LswControl(this, 5, "линейный автомат №5")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category3, "АЗ", "Контроль и управление", new Prism.Units.Controls.Lsw9Control(this, "запасной автомат")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category2, "ВА №1", "Контроль и управление", new Prism.Units.Controls.RectControl(this, 1, "выпрямительный агрегат №1")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category2, "ВА №2", "Контроль и управление", new Prism.Units.Controls.RectControl(this, 2, "выпрямительный агрегат №2")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category2, "ВА №3", "Контроль и управление", new Prism.Units.Controls.RectControl(this, 3, "выпрямительный агрегат №3")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category1, "Рабочий ввод", "Контроль и управление", new Prism.Units.Controls.LeadinControl(this, 1, "рабочий ввод")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category1, "Резервный ввод", "Контроль и управление", new Prism.Units.Controls.LeadinControl(this, 2, "резервный ввод")));
                    PresentationControlsInternal.Add(new PresentationControl(PresentationControlCategory.Category1, "Отходящая линия", "Контроль и управление", new Prism.Units.Controls.LeadinControl(this, 3, "отходящая линия")));
                    ContinueEvent.Set();
                });

                ContinueEvent.WaitOne();
                complete();
            }, null);
        }

        public void Uninitialize()
        {
        }

        public void BecomeActive(IPresentationControl presentationControl)
        {
        }

        public void ResignActive()
        {
        }

        private void ProcessingUpdateEvent(object sender)
        {
            try
            {
                Dictionary<string, Alarm> AlarmValues = new Dictionary<string, Alarm>(Processing.AlarmValues);
                ParamState newState = ParamState.Unknown;
                bool hasModifiedAlarms = false;
                int lastAlarmCount = AlarmsInternal.Count;

                AlarmsInternal.RemoveAll(delegate(Alarm a)
                {
                    return !Processing.AlarmValues.ContainsKey("alarm," + a.Code);
                });

                hasModifiedAlarms = lastAlarmCount > AlarmsInternal.Count;

                foreach (KeyValuePair<string, Alarm> alarmKeyValue in AlarmValues)
                {
                    Alarm lastAlarm = AlarmsInternal.Find(delegate(Alarm a)
                    {
                        return a.Code.Equals(alarmKeyValue.Value.Code);
                    });

                    if (lastAlarm != null)
                    {
                        if (lastAlarm.State != alarmKeyValue.Value.State || !lastAlarm.Description.Equals(alarmKeyValue.Value.Description))
                        {
                            lastAlarm.State = alarmKeyValue.Value.State;
                            lastAlarm.Description = alarmKeyValue.Value.Description;
                            hasModifiedAlarms = true;
                        }
                    }
                    else
                    {
                        hasModifiedAlarms = true;
                        AlarmsInternal.Add(alarmKeyValue.Value);
                    }

                    if (alarmKeyValue.Value.State > newState)
                    {
                        newState = alarmKeyValue.Value.State;
                    }
                }

                if (this.IsOnline)
                {
                    if (Processing.Params["common_state"].State > newState)
                    {
                        newState = Processing.Params["common_state"].State;
                    }
                }
                else
                {
                    newState = ParamState.B;
                }

                if (hasModifiedAlarms)
                {
                    if (UnitAlarmsChangedEvent != null)
                    {
                        MainThread.EnqueueTask(delegate()
                        {
                            UnitAlarmsChangedEvent(this, AlarmsInternal);
                        });
                    }
                }

                if (newState != LastState)
                {
                    LastState = newState;
                    if (UnitStateChangedEvent != null)
                    {
                        MainThread.EnqueueTask(delegate()
                        {
                            UnitStateChangedEvent(this, LastState);
                        });
                    }
                }
            }
            catch (Exception)
            {
            }            
        }

        private void ProcessingChangeStateEvent(object sender)
        {
            if (!this.IsOnline)
            {
                ParamState newState = ParamState.B;
                if (newState != LastState)
                {
                    LastState = newState;
                    if (UnitStateChangedEvent != null)
                    {
                        UnitStateChangedEvent(this, LastState);
                    }
                }
            }

            if (LastIsBusy != this.IsBusy)
            {
                LastIsBusy = this.IsBusy;
                if (UnitBusyStateChangedEvent != null)
                {
                    UnitBusyStateChangedEvent(this, LastIsBusy);
                }
            }            
        }
    }
}

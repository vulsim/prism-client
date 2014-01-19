using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Media;
using System.Windows;
using Prism.General;
using Prism.General.Automation;

namespace Prism.Classes
{
    public class NotificationAlarm
    {
        public Unit Unit { get; set; }
        public DateTime Date { get; set; }
        public ParamState State { get; set; }
        public string Prefix { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool Ack { get; set; }

        public NotificationAlarm(Unit unit, ParamState state, string code, string description, bool ack)
        {
            this.Unit = unit;
            this.Date = DateTime.Now;
            this.State = state;
            this.Prefix = "UN";
            this.Code = code;
            this.Description = description;
            this.Ack = ack;

            string[] codeComp = code.Split(new Char[] { '-' });

            if (codeComp.Length > 1)
            {
                this.Prefix = codeComp[0].Substring(0, 2).ToUpper();
            }
        }

        public NotificationAlarm(Unit unit, IAlarm alarm, bool ack)
        {
            this.Unit = unit;
            this.Date = DateTime.Now;
            this.State = alarm.State;
            this.Prefix = "UN";
            this.Code = alarm.Code;
            this.Description = alarm.Description;
            this.Ack = ack;

            string[] codeComp = alarm.Code.Split(new Char[] { '-' });

            if (codeComp.Length > 1)
            {
                this.Prefix = codeComp[0].Substring(0, 2).ToUpper();
            }
        }
    }

    public delegate void GeneralAlarmsStateChangedEventHandler(object sender);

    public class AlarmNotificationCenter
    {
        public static AlarmNotificationCenter Instance = null;

        public List<NotificationAlarm> alarms;
        public event GeneralAlarmsStateChangedEventHandler GeneralAlarmsStateChangedEvent;

        private System.Timers.Timer notificationTimer;
        private bool IsNotificationInProgress = false;
        private bool HasModifiedAlarms = false;

        public class DebugAlarm : IAlarm
        {
            public string Code { get; set; }
            public string Description { get; set; }
            public ParamState State { get; set; }

            public DebugAlarm(string code, string description, ParamState state)
            {
                Code = code;
                Description = description;
                State = state;
            }
        }

        public AlarmNotificationCenter()
        {
            this.alarms = new List<NotificationAlarm>();

            notificationTimer = new System.Timers.Timer(30000);
            notificationTimer.Elapsed += NotificationTimerEvent;
            notificationTimer.Start();
        }

        ~AlarmNotificationCenter()
        {
            notificationTimer.Stop();
        }

        public void Update(Unit unit)
        {            
            List<IAlarm> Alarms = new List<IAlarm>(unit.Alarms);
            List<NotificationAlarm> needRemove = new List<NotificationAlarm>();
            bool hasModifiedAlarms = false;

            foreach (var alarm in alarms)
            {
                if (alarm.Unit == unit)
                {
                    needRemove.Add(alarm);
                }
            }
            
            foreach (var newAlarm in Alarms)
            {
                bool alarmExists = false;

                foreach (var lastAlarm in alarms)
                {
                    if (lastAlarm.Unit == unit && newAlarm.Code.Equals(lastAlarm.Code))
                    {
                        needRemove.Remove(lastAlarm);
                        alarmExists = true;

                        if (newAlarm.State != lastAlarm.State || !newAlarm.Description.Equals(lastAlarm.Description))
                        {
                            if (newAlarm.State != lastAlarm.State)
                            {
                                lastAlarm.Ack = false;
                            }
                            lastAlarm.State = newAlarm.State;
                            lastAlarm.Description = newAlarm.Description;
                            hasModifiedAlarms = true;
                        }                        
                        break;
                    }
                }

                if (!alarmExists)
                {
                    NotificationAlarm notificationAlarm = new NotificationAlarm(unit, newAlarm, false);
                    alarms.Add(notificationAlarm);
                    hasModifiedAlarms = true;
                }
            }

            hasModifiedAlarms = hasModifiedAlarms || needRemove.Count > 0;
            this.HasModifiedAlarms = hasModifiedAlarms;

            foreach (var alarm in needRemove)
            {
                alarms.Remove(alarm);                
            }

            if (hasModifiedAlarms && GeneralAlarmsStateChangedEvent != null)
            {
                MainThread.EnqueueTask(delegate()
                {
                    GeneralAlarmsStateChangedEvent(this);
                });         
            }

            MainThread.EnqueueTask(delegate()
            {
                NotificationTimerEvent(this, null);
            });
        }

        private void NotificationTimerEvent(object sender, ElapsedEventArgs e)
        {
            if (IsNotificationInProgress)
            {
                return;
            }

            IsNotificationInProgress = true;
            notificationTimer.Stop();

            foreach (var alarm in alarms)
            {
                if (alarm.State == ParamState.C && !alarm.Ack)
                {
                    Uri buzzerUri = new Uri(@"pack://application:,,,/prism;Component/Resources/alarm.buzzer.wav");
                    SoundPlayer buzzerPlayer = new SoundPlayer(Application.GetResourceStream(buzzerUri).Stream);
                    buzzerPlayer.Play();

                    IsNotificationInProgress = false;
                    notificationTimer.Start();
                    return;
                }
            }

            if (this.HasModifiedAlarms)
            {
                this.HasModifiedAlarms = false;

                Uri notifyUri = new Uri(@"pack://application:,,,/prism;Component/Resources/alarm.notify.wav");
                SoundPlayer notifyPlayer = new SoundPlayer(Application.GetResourceStream(notifyUri).Stream);
                notifyPlayer.Play();
            }

            IsNotificationInProgress = false;
            notificationTimer.Start();
        }
    }
}

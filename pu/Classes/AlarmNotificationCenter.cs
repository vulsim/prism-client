using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Media;
using System.Windows;
using System.Threading;
using System.Speech.Synthesis;
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

        public List<NotificationAlarm> Alarms;
        public event GeneralAlarmsStateChangedEventHandler GeneralAlarmsStateChangedEvent;

        private System.Timers.Timer LongNotificationTimer;
        private System.Timers.Timer ShortNotificationTimer;        
        private bool CanPlayShortNotification = false;
        private bool CanPlayLongNotification = true;

        //private bool IsNotificationInProgress = false;
        

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
            Alarms = new List<NotificationAlarm>();

            ShortNotificationTimer = new System.Timers.Timer(3000);
            ShortNotificationTimer.Elapsed += ShortNotificationTimerEvent;
            ShortNotificationTimer.Start();

            LongNotificationTimer = new System.Timers.Timer(30000);
            LongNotificationTimer.Elapsed += LongNotificationTimerEvent;
            LongNotificationTimer.Start();
        }

        ~AlarmNotificationCenter()
        {
            ShortNotificationTimer.Elapsed -= ShortNotificationTimerEvent;
            LongNotificationTimer.Elapsed -= LongNotificationTimerEvent;
        }

        public void UpdateAlarm(Unit unit, IAlarm alarm)
        {
            foreach (var notificationAlarm in Alarms)
            {
                if (notificationAlarm.Code.Equals(alarm.Code))
                {
                    if (notificationAlarm.State != alarm.State)
                    {
                        if (alarm.State == ParamState.Idle)
                        {
                            Alarms.Remove(notificationAlarm);

                            if (GeneralAlarmsStateChangedEvent != null)
                            {
                                GeneralAlarmsStateChangedEvent(this);
                            }

                            CanPlayShortNotification = true;
                            return;
                        }

                        notificationAlarm.State = alarm.State;
                        notificationAlarm.Ack = false;

                        if (GeneralAlarmsStateChangedEvent != null)
                        {
                            GeneralAlarmsStateChangedEvent(this);
                        }

                        CanPlayShortNotification = true;

                        if (notificationAlarm.State == ParamState.C && CanPlayLongNotification)
                        {
                            LongNotificationTimer.Stop();
                            LongNotificationTimer.Interval = 3000;
                            LongNotificationTimer.Start();
                        }
                    }

                    return;
                }
            }
            
            if (alarm.State > ParamState.Idle)
            {
                NotificationAlarm notificationAlarm = new NotificationAlarm(unit, alarm, false);
                Alarms.Add(notificationAlarm);

                if (GeneralAlarmsStateChangedEvent != null)
                {
                    GeneralAlarmsStateChangedEvent(this);
                }

                CanPlayShortNotification = true;

                if (notificationAlarm.State == ParamState.C && CanPlayLongNotification)
                {
                    LongNotificationTimer.Stop();
                    LongNotificationTimer.Interval = 3000;
                    LongNotificationTimer.Start();
                }
            }                        
        }

        private void ShortNotificationTimerEvent(object sender, ElapsedEventArgs e)
        {
            ShortNotificationTimer.Stop();

            if (CanPlayShortNotification)
            {
                PlayNotify();
                CanPlayShortNotification = false;
            }
            ShortNotificationTimer.Start();
        }

        private void LongNotificationTimerEvent(object sender, ElapsedEventArgs e)
        {
            LongNotificationTimer.Stop();
            CanPlayLongNotification = false;

            List<string> alarmMessages = new List<string>();

            foreach (var notificationAlarm in Alarms)
            {
                if (notificationAlarm.State == ParamState.C && !notificationAlarm.Ack)
                {
                    alarmMessages.Add(String.Format("Внимание. {0}, {1}", notificationAlarm.Unit.FullName, notificationAlarm.Description));
                }
            }

            if (alarmMessages.Count > 0)
            {
                PlayBuzzer();
                Thread.Sleep(1000);

                try
                {
                    SpeechSynthesizer Synthesizer = new SpeechSynthesizer();
                    foreach (var message in alarmMessages)
                    {
                        Synthesizer.Speak(message);
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception exception)
                {
                }                
            }

            CanPlayLongNotification = true;
            LongNotificationTimer.Interval = 30000;
            LongNotificationTimer.Start();
        }

        private void PlayNotify()
        {
            Uri notifyUri = new Uri(@"pack://application:,,,/prism;Component/Resources/alarm.notify.wav");
            SoundPlayer soundPlayer = new SoundPlayer(Application.GetResourceStream(notifyUri).Stream);

            soundPlayer.PlaySync();
        }

        private void PlayBuzzer()
        {
            Uri buzzerUri = new Uri(@"pack://application:,,,/prism;Component/Resources/alarm.buzzer.wav");
            SoundPlayer soundPlayer = new SoundPlayer(Application.GetResourceStream(buzzerUri).Stream);
            
            soundPlayer.PlaySync();
        }        
    }
}

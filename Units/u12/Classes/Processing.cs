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
    public delegate void ProcessingParamUpdateEventHandler(object sender, Param param);
    public delegate void ProcessingAlarmUpdateEventHandler(object sender, Alarm alarm);
    public delegate void ProcessingOperateCallback(string error, ProducerChannelValue value);
    public delegate void ProcessingOnlineStateChangedEventHandler(object sender, bool isOnline);

    public class Processing
    {
        private Unit Unit;
        private PrismRequestProducer RequestProducer = null;
        private PrismPollProducer PollProducer = null;
        private PrismSubscribeProducer SubscribeProducer = null;
        private System.Timers.Timer UpdateTimer;
        private Thread PollThread;
        private ManualResetEvent PollDoneEvent;
        private bool PollThreadAborted = false;
        private bool lastOnlineState = false;

        public bool IsOnline { get { return lastOnlineState; } }
        public List<ProducerChannel> Channels;
        public Dictionary<string, string> Values;
        public Dictionary<string, Param> Params;
        public Dictionary<string, Alarm> Alarms;
        public Dictionary<string, List<Param>> ParamRelations;

        public event ProcessingParamUpdateEventHandler ProcessingParamUpdateEvent;
        public event ProcessingAlarmUpdateEventHandler ProcessingAlarmUpdateEvent;
        public event ProcessingOnlineStateChangedEventHandler ProcessingOnlineStateChangedEvent;

        public Processing(Unit unit)
        {
            Unit = unit;
            
            Channels = new List<General.Automation.ProducerChannel>();
            Values = new Dictionary<string, string>();           
            Alarms = new Dictionary<string, Alarm>();
            Params = new Dictionary<string, Param>();

            Configure();

            ParamRelations = new Dictionary<string,List<Param>>();
            foreach (var channel in Channels)
            {
                string key = String.Format("{0},{1}", channel.Group, channel.Channel);

                foreach (KeyValuePair<string, Param> keyValue in Params)
                {
                    if (keyValue.Value.RealatedTo(key))                    
                    {
                        if (!ParamRelations.ContainsKey(key))
                        {
                            ParamRelations[key] = new List<Param>();
                        }

                        ParamRelations[key].Add(keyValue.Value);
                    }
                }
            }

            UpdateTimer = new System.Timers.Timer();
            UpdateTimer.Elapsed += ProducerUpdateTimerEvent;

            List<string> subscribe = new List<string>();
            subscribe.Add("alarm");

            foreach (var channel in Channels)
            {
                subscribe.Add(String.Format("{0},{1}", channel.Group, channel.Channel));
            }

            SubscribeProducer = new PrismSubscribeProducer(Unit.Settings.SubscribeEndpoint, subscribe);
            SubscribeProducer.ChannelValueEvent += ProducerChannelValueEvent;
            SubscribeProducer.ChannelResetEvent += ProducerChannelResetEvent;

            Unit.Manager.PollManagerPauseEvent += PollPause;
            Unit.Manager.PollManagerResumeEvent += PollResume;

            Unit.Manager.EnqueuePoll(Unit, Poll, PollTerminate);
        }

        ~Processing()
        {
            PollThreadAborted = true;
            PollTerminate();

            UpdateTimer.Elapsed -= ProducerUpdateTimerEvent;
            UpdateTimer.Stop();

            SubscribeProducer.ChannelValueEvent -= ProducerChannelValueEvent;
            SubscribeProducer.ChannelResetEvent -= ProducerChannelResetEvent;

            Unit.Manager.PollManagerPauseEvent -= PollPause;
            Unit.Manager.PollManagerResumeEvent -= PollResume;
        }

        private void PollPause(object sender)
        {
            if (RequestProducer != null)
            {
                RequestProducer.Pause();
            }

            if (PollProducer != null)
            {
                PollProducer.Pause();
            }
        }

        private void PollResume(object sender)
        {
            if (RequestProducer != null)
            {
                RequestProducer.Resume();
            }

            if (PollProducer != null)
            {
                PollProducer.Resume();
            }
        }

        private void ProducerUpdateTimerEvent(object sender, ElapsedEventArgs e)
        {
            Unit.Manager.EnqueuePoll(Unit, Poll, PollTerminate);
            UpdateTimer.Stop();
        }

        private void ProducerChannelValueEvent(object sender, ProducerChannelValue value)
        {
            UpdateChannel(value);
        }

        private void ProducerChannelResetEvent(object sender, ProducerChannel channel)
        {
            ResetChannel(channel);
        }

        public void Operate(ProducerChannelValue value, TimeSpan time, ProcessingOperateCallback cb)
        {
            PrismRequestProducer operateProducer = new PrismRequestProducer(new List<string> { Unit.Settings.OperateEndpoint });
            bool canCallCb = true;

            /*Unit.Manager.Silent(time, delegate()
            {
                if (canCallCb)
                {
                    cb("Waiting for a response over quota", new ProducerChannelValue(value.Group, value.Channel, null));
                    canCallCb = false;
                }
            });*/

            operateProducer.WriteChannelValue(value, delegate(string error, ProducerChannelValue value1)
            {
                if (canCallCb)
                {
                    cb(error, value1);
                    canCallCb = false;
                }

                operateProducer.Terminate();
            });            
        }

        private void UpdateChannel(ProducerChannelValue value)
        {
            string key = String.Format("{0},{1}", value.Group, value.Channel);

            if (value.Group.Equals("alarm"))
            {
                if (!value.Channel.Contains("-manual"))
                {          
                    bool update = false;
                    Alarm alarm = GetAlarm(value);

                    if (Alarms.ContainsKey(key))
                    {
                        if (alarm.State != Alarms[key].State)
                        {
                            update = true;
                        }                        
                    }
                    else
                    {
                        update = true;
                    }

                    if (update)
                    {
                        Alarms[key] = alarm;
                        Unit.Journal.Log(Unit, alarm);

                        ThreadPool.QueueUserWorkItem(delegate(object target)
                        {
                            if (ProcessingAlarmUpdateEvent != null)
                            {
                                ProcessingAlarmUpdateEvent(this,(Alarm)target);
                            }
                        }, alarm);
                    }
                }
            }
            else
            {                
                bool update = false;

                if (Values.ContainsKey(key))
                {
                    if (!Values[key].Equals(value.Value))
                    {
                        update = true;
                    }
                }
                else
                {
                    update = true;
                }

                if (update)
                {
                    Values[key] = value.Value;

                    if (ParamRelations.ContainsKey(key))
                    {
                        foreach (var param in ParamRelations[key])
                        {
                            ThreadPool.QueueUserWorkItem(delegate(object target)
                            {
                                if (ProcessingParamUpdateEvent != null)
                                {
                                    ProcessingParamUpdateEvent(this, (Param)target);
                                }
                            }, param);
                        }
                    }
                }
            }
        }

        private void ResetChannel(ProducerChannel channel)
        {
            ResetChannel(String.Format("{0},{1}", channel.Group, channel.Channel));
        }

        private void ResetChannel(string key)
        {
            if (key.Contains("alarm,"))
            {
                if (!key.Contains("-manual"))
                {
                    if (Alarms.ContainsKey(key))
                    {
                        Alarm alarm = Alarms[key];
                        alarm.State = ParamState.Idle;

                        Alarms.Remove(key);
                        Unit.Journal.Log(Unit, alarm);

                        ThreadPool.QueueUserWorkItem(delegate(object target)
                        {
                            if (ProcessingAlarmUpdateEvent != null)
                            {
                                ProcessingAlarmUpdateEvent(this, (Alarm)target);
                            }
                        }, alarm);
                    }
                }
            }
            else
            {
                if (Values.ContainsKey(key))
                {
                    Values.Remove(key);

                    if (ParamRelations.ContainsKey(key))
                    {
                        foreach (var param in ParamRelations[key])
                        {
                            ThreadPool.QueueUserWorkItem(delegate(object target)
                            {
                                if (ProcessingParamUpdateEvent != null)
                                {
                                    ProcessingParamUpdateEvent(this, (Param)target);
                                }
                            }, param);
                        }
                    }
                }
            }            
        }

        private void PollTerminate()
        {
            if (PollThread != null)
            {
                try
                {
                    PollDoneEvent.Set();
                    PollThread.Join();
                }
                catch (Exception e)
                {
                }
            }            
        }

        private void Poll(PollManagerCompleteHandler complete)
        {
            RequestProducer = new PrismRequestProducer(new List<string> { Unit.Settings.CommonEndpoint });
            PollProducer = new PrismPollProducer(Unit.Settings.PollEndpoint);
            PollDoneEvent = new ManualResetEvent(false);
            PollThread = new Thread(delegate()
            {
                List<ProducerChannel> pollingChannelList = new List<ProducerChannel>();
                List<ProducerChannel> pollingAlarmList = new List<ProducerChannel>();

                int TotalChannelCount = 0;
                int SuccessChannelCount = 0;

                PollDoneEvent.Reset();
                RequestProducer.GetChannelList("alarm", delegate(string error0, List<ProducerChannel> channels)
                {
                    if (PollThreadAborted)
                    {
                        PollDoneEvent.Set();
                        return;
                    }

                    if (error0 == null)
                    {
                        List<string> expiredAlarms = new List<string>(Alarms.Keys);

                        foreach (var channel in channels)
                        {
                            if (!channel.Channel.Contains("-manual"))
                            {
                                expiredAlarms.Remove(String.Format("{0},{1}", channel.Group, channel.Channel));
                                pollingAlarmList.Add(channel);
                            }
                        }

                        foreach (var alarmKey in expiredAlarms)
                        {
                            ResetChannel(alarmKey);
                        }

                        PollDoneEvent.Set();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(error0.ToString());
                    }
                });

                PollDoneEvent.WaitOne();

                if (PollThreadAborted)
                {
                    return;
                }

                PollDoneEvent.Reset();
                RequestProducer.GetChannelList("io", delegate(string error0, List<ProducerChannel> channels)
                {
                    if (PollThreadAborted)
                    {
                        PollDoneEvent.Set();
                        return;
                    }

                    if (error0 == null)
                    {
                        List<string> expiredChannels = new List<string>(Alarms.Keys);

                        foreach (var channel in channels)
                        {
                            expiredChannels.Remove(String.Format("{0},{1}", channel.Group, channel.Channel));
                            pollingChannelList.Add(channel);
                        }

                        foreach (var channelKey in expiredChannels)
                        {
                            ResetChannel(channelKey);
                        }

                        PollDoneEvent.Set();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(error0.ToString());
                    }
                });

                PollDoneEvent.WaitOne();

                if (PollThreadAborted)
                {
                    return;
                }

                TotalChannelCount += pollingAlarmList.Count;
                TotalChannelCount += pollingChannelList.Count;

                PollDoneEvent.Reset();
                PollProducer.Enqueue(pollingAlarmList, delegate(string error1, List<ProducerChannelValue> alarmValueList)
                {
                    foreach (var value in alarmValueList)
                    {
                        UpdateChannel(value);
                    }

                    SuccessChannelCount += alarmValueList.Count;
                    PollProducer.Enqueue(pollingChannelList, delegate(string error2, List<ProducerChannelValue> channelValueList)
                    {
                        foreach (var value in channelValueList)
                        {
                            UpdateChannel(value);
                        }

                        SuccessChannelCount += channelValueList.Count;
                        PollDoneEvent.Set();
                    });
                });

                PollDoneEvent.WaitOne();

                if (PollThreadAborted)
                {
                    return;
                }

                bool newOnlineState = ((double)SuccessChannelCount / (double)TotalChannelCount > 0.85f);

                if (lastOnlineState != newOnlineState)
                {
                    lastOnlineState = newOnlineState;

                    if (ProcessingOnlineStateChangedEvent != null)
                    {
                        ProcessingOnlineStateChangedEvent(this, lastOnlineState);
                    }
                }

                if (SuccessChannelCount == TotalChannelCount)
                {
                    UpdateTimer.Interval = 300000;
                }
                else
                {
                    UpdateTimer.Interval = 60000;
                }

                UpdateTimer.Start();
                complete();

                RequestProducer.Terminate();
                RequestProducer = null;
                PollDoneEvent = null;
                PollThread = null;
            });
            PollThread.Start();
        }

        /*private void Poll(PollManagerCompleteHandler complete)
        {
            RequestProducer = new PrismRequestProducer(Unit.Settings.PollEndpoints);
            PollDoneEvent = new ManualResetEvent(false);
            PollThread = new Thread(delegate()
            {
                List<ProducerChannel> pollingChannelList = new List<ProducerChannel>();
                List<ProducerChannel> pollingAlarmList = new List<ProducerChannel>();

                int TotalChannelCount = 0;
                int SuccessChannelCount = 0;
                int ProcessedChannelCount = 0;

                PollDoneEvent.Reset();
                RequestProducer.GetChannelList("alarm", delegate(string error0, List<ProducerChannel> channels)
                {
                    if (PollThreadAborted)
                    {
                        PollDoneEvent.Set();
                        return;
                    }

                    if (error0 == null)
                    {
                        List<string> expiredAlarms = new List<string>(Alarms.Keys);

                        foreach (var channel in channels)
                        {
                            if (!channel.Channel.Contains("-manual"))
                            {
                                expiredAlarms.Remove(String.Format("{0},{1}", channel.Group, channel.Channel));
                                pollingAlarmList.Add(channel);
                            }
                        }

                        foreach (var alarmKey in expiredAlarms)
                        {
                            ResetChannel(alarmKey);
                        }

                        PollDoneEvent.Set();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(error0.ToString());
                    }
                });

                PollDoneEvent.WaitOne();

                if (PollThreadAborted)
                {
                    return;
                }

                PollDoneEvent.Reset();
                RequestProducer.GetChannelList("io", delegate(string error0, List<ProducerChannel> channels)
                {
                    if (PollThreadAborted)
                    {
                        PollDoneEvent.Set();
                        return;
                    }

                    if (error0 == null)
                    {
                        List<string> expiredChannels = new List<string>(Alarms.Keys);

                        foreach (var channel in channels)
                        {
                            expiredChannels.Remove(String.Format("{0},{1}", channel.Group, channel.Channel));
                            pollingChannelList.Add(channel);
                        }

                        foreach (var channelKey in expiredChannels)
                        {
                            ResetChannel(channelKey);
                        }

                        PollDoneEvent.Set();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(error0.ToString());
                    }
                });

                PollDoneEvent.WaitOne();

                if (PollThreadAborted)
                {
                    return;
                }

                TotalChannelCount += pollingAlarmList.Count;
                TotalChannelCount += pollingChannelList.Count;

                PollDoneEvent.Reset();
                foreach (var channel in pollingAlarmList)
                {
                    RequestProducer.ReadChannelValue(channel, delegate(string error1, ProducerChannelValue value)
                    {
                        ProcessedChannelCount++;
                        //System.Diagnostics.Debug.WriteLine(ProcessedChannelCount.ToString());

                        if (error1 == null)
                        {
                            SuccessChannelCount++;
                            UpdateChannel(value);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(error1.ToString());
                        }

                        if (ProcessedChannelCount == TotalChannelCount)
                        {
                            PollDoneEvent.Set();
                        }
                    });
                }

                foreach (var channel in pollingChannelList)
                {
                    RequestProducer.ReadChannelValue(channel, delegate(string error2, ProducerChannelValue value)
                    {
                        string key = value.Group + "," + value.Channel;

                        ProcessedChannelCount++;
                        //System.Diagnostics.Debug.WriteLine(ProcessedChannelCount.ToString());

                        if (error2 == null)
                        {
                            SuccessChannelCount++;
                            UpdateChannel(value);
                        }
                        else
                        {
                            ResetChannel(new ProducerChannel(value.Group, value.Channel));
                            System.Diagnostics.Debug.WriteLine(error2.ToString());
                        }

                        if (ProcessedChannelCount == TotalChannelCount)
                        {
                            PollDoneEvent.Set();
                        }
                    });
                }

                PollDoneEvent.WaitOne();

                if (PollThreadAborted)
                {
                    return;
                }

                bool newOnlineState = ((double)SuccessChannelCount / (double)TotalChannelCount > 0.85f);

                if (lastOnlineState != newOnlineState)
                {
                    lastOnlineState = newOnlineState;

                    if (ProcessingOnlineStateChangedEvent != null)
                    {
                        ProcessingOnlineStateChangedEvent(this, lastOnlineState);
                    }
                }

                if (SuccessChannelCount == TotalChannelCount)
                {
                    UpdateTimer.Interval = 300000;
                }
                else
                {
                    UpdateTimer.Interval = 60000;
                }

                UpdateTimer.Start();
                complete();

                RequestProducer.Terminate();
                RequestProducer = null;
                PollDoneEvent = null;
                PollThread = null;
            });
            PollThread.Start();
        }*/
        
        private Alarm GetAlarm(ProducerChannelValue value)
        {
            if (value.Group.Equals("alarm"))
            {
                string[] splitValue = value.Value.Split(" ".ToCharArray(), 2);

                if (splitValue != null && splitValue.Length == 2)
                {
                    ParamState paramState = ParamState.Unknown;

                    if (splitValue[0].Equals("[I]"))
                    {
                        paramState = ParamState.Idle;
                    }
                    else if (splitValue[0].Equals("[A]"))
                    {
                        paramState = ParamState.A;
                    }
                    else if (splitValue[0].Equals("[B]"))
                    {
                        paramState = ParamState.B;
                    }
                    if (splitValue[0].Equals("[C]"))
                    {
                        paramState = ParamState.C;
                    }

                    return new Alarm(value.Channel, splitValue[1], paramState);
                }
            }

            return null;
        }

        /// <summary>
        /// Конфигурация параметров и каналов подстанции
        /// </summary>

        private void Configure()
        {
            Params["leadin1_state_in_switch"] = new Param("leadin1_state_in_switch", Values, "io,di-rab-908", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["leadin1_state_tc_switch"] = new Param("leadin1_state_tc_switch", Values, "io,di-rab-916", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "1"), new ParamMapValue(ParamState.B, "0") }));
            Params["leadin1_alarm_in_switch_fault"] = new Param("leadin1_alarm_in_switch_fault", Values, "io,di-rab-910", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin1_alarm_circuit_fault"] = new Param("leadin1_alarm_circuit_fault", Values, "io,di-rab-912", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin1_alarm_tn_circuit_fault"] = new Param("leadin1_alarm_tn_circuit_fault", Values, "io,di-tn1-918", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin1_alarm_tn_ru6kv_fault"] = new Param("leadin1_alarm_tn_ru6kv_fault", Values, "io,di-tn1-920", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin1_alarm_tsn_lost_power"] = new Param("leadin1_alarm_tsn_lost_power", Values, "io,di-tsn1-ts71", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["leadin1_state"] = new Param("leadin1_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state_in_switch"], ParamState.Unknown), 
                    new ParamCombination(Params["leadin1_alarm_in_switch_fault"], ParamState.Unknown),
                    new ParamCombination(Params["leadin1_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["leadin1_alarm_tn_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["leadin1_alarm_tn_ru6kv_fault"], ParamState.Unknown),
                    new ParamCombination(Params["leadin1_alarm_tsn_lost_power"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state_in_switch"], ParamState.A), 
                    new ParamCombination(Params["leadin1_alarm_in_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tn_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tn_ru6kv_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tsn_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state_in_switch"], ParamState.B), 
                    new ParamCombination(Params["leadin1_alarm_in_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tn_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tn_ru6kv_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tsn_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["leadin2_state_in_switch"] = new Param("leadin2_state_in_switch", Values, "io,di-rez-900", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["leadin2_state_tc_switch"] = new Param("leadin2_state_tc_switch", Values, "io,di-rez-906", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["leadin2_alarm_in_switch_fault"] = new Param("leadin2_alarm_in_switch_fault", Values, "io,di-rez-902", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin2_alarm_circuit_fault"] = new Param("leadin2_alarm_circuit_fault", Values, "io,di-rez-904", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin2_alarm_tn_circuit_fault"] = new Param("leadin2_alarm_tn_circuit_fault", Values, "io,di-tn2-918", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin2_alarm_tn_ru6kv_fault"] = new Param("leadin2_alarm_tn_ru6kv_fault", Values, "io,di-tn2-741", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin2_alarm_tsn_lost_power"] = new Param("leadin2_alarm_tsn_lost_power", Values, "io,di-tsn2-ts71", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["leadin2_state"] = new Param("leadin2_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin2_state_in_switch"], ParamState.Unknown), 
                    new ParamCombination(Params["leadin2_alarm_in_switch_fault"], ParamState.Unknown),
                    new ParamCombination(Params["leadin2_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["leadin2_alarm_tn_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["leadin2_alarm_tn_ru6kv_fault"], ParamState.Unknown),
                    new ParamCombination(Params["leadin2_alarm_tsn_lost_power"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin2_state_in_switch"], ParamState.A), 
                    new ParamCombination(Params["leadin2_alarm_in_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tn_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tn_ru6kv_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tsn_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin2_state_in_switch"], ParamState.B), 
                    new ParamCombination(Params["leadin2_alarm_in_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tn_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tn_ru6kv_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tsn_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["ol_state_in_switch"] = new Param("ol_state_in_switch", Values, "io,di-ol-908", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["ol_state_tc_switch"] = new Param("ol_state_tc_switch", Values, "io,di-ol-916", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "1"), new ParamMapValue(ParamState.B, "0") }));
            Params["ol_alarm_switch_fault"] = new Param("ol_alarm_switch_fault", Values, "io,di-ol-910", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["ol_alarm_circuit_fault"] = new Param("ol_alarm_circuit_fault", Values, "io,di-ol-912", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["ol_state"] = new Param("ol_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["ol_state_in_switch"], ParamState.Unknown), 
                    new ParamCombination(Params["ol_alarm_switch_fault"], ParamState.Unknown),
                    new ParamCombination(Params["ol_alarm_circuit_fault"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["ol_state_in_switch"], ParamState.A), 
                    new ParamCombination(Params["ol_alarm_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["ol_alarm_circuit_fault"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["ol_state_in_switch"], ParamState.B), 
                    new ParamCombination(Params["ol_alarm_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["ol_alarm_circuit_fault"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["rect1_state_pa_switch"] = new Param("rect1_state_pa_switch", Values, "io,di-pa1-911", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["rect1_state_qs_switch"] = new Param("rect1_state_qs_switch", Values, "io,di-ru1-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect1_state_qf_switch"] = new Param("rect1_state_qf_switch", Values, "io,di-ru1-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect1_state_tc_switch"] = new Param("rect1_state_tc_switch", Values, "io,di-ru1-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect1_alarm_circuit_fault"] = new Param("rect1_alarm_circuit_fault", Values, "io,di-ka1-n02", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_pa_switch_fault"] = new Param("rect1_alarm_pa_switch_fault", Values, "io,di-pa1-1003", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_rec_fault"] = new Param("rect1_alarm_rec_fault", Values, "io,di-v1-67", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_rec_gas_warn"] = new Param("rect1_alarm_rec_gas_warn", Values, "io,di-v1-86", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_rec_overload"] = new Param("rect1_alarm_rec_overload", Values, "io,di-v1-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_rec_rpz600v_fault"] = new Param("rect1_alarm_rec_rpz600v_fault", Values, "io,di-v1-106", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["rect1_state"] = new Param("rect1_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state_pa_switch"], ParamState.Unknown),
                    new ParamCombination(Params["rect1_state_qs_switch"], ParamState.Unknown),
                    new ParamCombination(Params["rect1_state_qf_switch"], ParamState.Unknown),
                    new ParamCombination(Params["rect1_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["rect1_alarm_pa_switch_fault"], ParamState.Unknown),
                    new ParamCombination(Params["rect1_alarm_rec_fault"], ParamState.Unknown),
                    new ParamCombination(Params["rect1_alarm_rec_gas_warn"], ParamState.Unknown),
                    new ParamCombination(Params["rect1_alarm_rec_overload"], ParamState.Unknown),
                    new ParamCombination(Params["rect1_alarm_rec_rpz600v_fault"], ParamState.Unknown)             
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state_pa_switch"], ParamState.A),
                    new ParamCombination(Params["rect1_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["rect1_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["rect1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_rpz600v_fault"], ParamState.Idle)             
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_rpz600v_fault"], ParamState.Idle)                
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["rect2_state_pa_switch"] = new Param("rect2_state_pa_switch", Values, "io,di-pa2-911", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["rect2_state_qs_switch"] = new Param("rect2_state_qs_switch", Values, "io,di-ru2-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect2_state_qf_switch"] = new Param("rect2_state_qf_switch", Values, "io,di-ru2-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect2_state_tc_switch"] = new Param("rect2_state_tc_switch", Values, "io,di-ru2-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect2_alarm_circuit_fault"] = new Param("rect2_alarm_circuit_fault", Values, "io,di-ka2-n02", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_pa_switch_fault"] = new Param("rect2_alarm_pa_switch_fault", Values, "io,di-pa2-1003", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_rec_fault"] = new Param("rect2_alarm_rec_fault", Values, "io,di-v2-67", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_rec_gas_warn"] = new Param("rect2_alarm_rec_gas_warn", Values, "io,di-v2-86", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_rec_overload"] = new Param("rect2_alarm_rec_overload", Values, "io,di-v2-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_rec_rpz600v_fault"] = new Param("rect2_alarm_rec_rpz600v_fault", Values, "io,di-v2-106", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["rect2_state"] = new Param("rect2_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect2_state_pa_switch"], ParamState.Unknown),
                    new ParamCombination(Params["rect2_state_qs_switch"], ParamState.Unknown),
                    new ParamCombination(Params["rect2_state_qf_switch"], ParamState.Unknown),
                    new ParamCombination(Params["rect2_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["rect2_alarm_pa_switch_fault"], ParamState.Unknown),
                    new ParamCombination(Params["rect2_alarm_rec_fault"], ParamState.Unknown),
                    new ParamCombination(Params["rect2_alarm_rec_gas_warn"], ParamState.Unknown),
                    new ParamCombination(Params["rect2_alarm_rec_overload"], ParamState.Unknown),
                    new ParamCombination(Params["rect2_alarm_rec_rpz600v_fault"], ParamState.Unknown)             
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect2_state_pa_switch"], ParamState.A),
                    new ParamCombination(Params["rect2_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["rect2_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["rect2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_rpz600v_fault"], ParamState.Idle)             
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_rpz600v_fault"], ParamState.Idle)                
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["rect3_state_pa_switch"] = new Param("rect3_state_pa_switch", Values, "io,di-pa3-911", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["rect3_state_qs_switch"] = new Param("rect3_state_qs_switch", Values, "io,di-ru3-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect3_state_qf_switch"] = new Param("rect3_state_qf_switch", Values, "io,di-ru3-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect3_state_tc_switch"] = new Param("rect3_state_tc_switch", Values, "io,di-ru3-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect3_alarm_circuit_fault"] = new Param("rect3_alarm_circuit_fault", Values, "io,di-ka3-n02", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_pa_switch_fault"] = new Param("rect3_alarm_pa_switch_fault", Values, "io,di-pa3-1003", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_rec_fault"] = new Param("rect3_alarm_rec_fault", Values, "io,di-v3-67", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_rec_gas_warn"] = new Param("rect3_alarm_rec_gas_warn", Values, "io,di-v3-86", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_rec_overload"] = new Param("rect3_alarm_rec_overload", Values, "io,di-v3-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_rec_rpz600v_fault"] = new Param("rect3_alarm_rec_rpz600v_fault", Values, "io,di-v3-106", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["rect3_state"] = new Param("rect3_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect3_state_pa_switch"], ParamState.Unknown),
                    new ParamCombination(Params["rect3_state_qs_switch"], ParamState.Unknown),
                    new ParamCombination(Params["rect3_state_qf_switch"], ParamState.Unknown),
                    new ParamCombination(Params["rect3_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["rect3_alarm_pa_switch_fault"], ParamState.Unknown),
                    new ParamCombination(Params["rect3_alarm_rec_fault"], ParamState.Unknown),
                    new ParamCombination(Params["rect3_alarm_rec_gas_warn"], ParamState.Unknown),
                    new ParamCombination(Params["rect3_alarm_rec_overload"], ParamState.Unknown),
                    new ParamCombination(Params["rect3_alarm_rec_rpz600v_fault"], ParamState.Unknown)             
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect3_state_pa_switch"], ParamState.A),
                    new ParamCombination(Params["rect3_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["rect3_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["rect3_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_rpz600v_fault"], ParamState.Idle)             
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect3_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_rpz600v_fault"], ParamState.Idle)                
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw1_state_qs_switch"] = new Param("lsw1_state_qs_switch", Values, "io,di-ul1-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw1_state_qf_switch"] = new Param("lsw1_state_qf_switch", Values, "io,di-ul1-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw1_state_tc_switch"] = new Param("lsw1_state_tc_switch", Values, "io,di-ul1-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw1_state_spare_switch"] = new Param("lsw1_state_spare_switch", Values, "io,di-ul1-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw1_alarm_short_fault"] = new Param("lsw1_alarm_short_fault", Values, "io,di-ul1-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw1_alarm_circuit_fault"] = new Param("lsw1_alarm_circuit_fault", Values, "io,di-ul1-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw1_alarm_600v_lost_power"] = new Param("lsw1_alarm_600v_lost_power", Values, "io,di-ul1-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["lsw1_state"] = new Param("lsw1_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state_qs_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw1_state_qf_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw1_alarm_short_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw1_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw1_alarm_600v_lost_power"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw1_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw1_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw1_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw1_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw2_state_qs_switch"] = new Param("lsw2_state_qs_switch", Values, "io,di-ul2-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw2_state_qf_switch"] = new Param("lsw2_state_qf_switch", Values, "io,di-ul2-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw2_state_tc_switch"] = new Param("lsw2_state_tc_switch", Values, "io,di-ul2-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw2_state_spare_switch"] = new Param("lsw2_state_spare_switch", Values, "io,di-ul2-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw2_alarm_short_fault"] = new Param("lsw2_alarm_short_fault", Values, "io,di-ul2-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw2_alarm_circuit_fault"] = new Param("lsw2_alarm_circuit_fault", Values, "io,di-ul2-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw2_alarm_600v_lost_power"] = new Param("lsw2_alarm_600v_lost_power", Values, "io,di-ul2-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["lsw2_state"] = new Param("lsw2_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw2_state_qs_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw2_state_qf_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw2_alarm_short_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw2_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw2_alarm_600v_lost_power"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw2_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw2_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw2_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw2_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw2_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw2_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw3_state_qs_switch"] = new Param("lsw3_state_qs_switch", Values, "io,di-ul3-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw3_state_qf_switch"] = new Param("lsw3_state_qf_switch", Values, "io,di-ul3-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw3_state_tc_switch"] = new Param("lsw3_state_tc_switch", Values, "io,di-ul3-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw3_state_spare_switch"] = new Param("lsw3_state_spare_switch", Values, "io,di-ul3-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw3_alarm_short_fault"] = new Param("lsw3_alarm_short_fault", Values, "io,di-ul3-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw3_alarm_circuit_fault"] = new Param("lsw3_alarm_circuit_fault", Values, "io,di-ul3-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw3_alarm_600v_lost_power"] = new Param("lsw3_alarm_600v_lost_power", Values, "io,di-ul3-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["lsw3_state"] = new Param("lsw3_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw3_state_qs_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw3_state_qf_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw3_alarm_short_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw3_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw3_alarm_600v_lost_power"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw3_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw3_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw3_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw3_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw3_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw3_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw3_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw3_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw4_state_qs_switch"] = new Param("lsw4_state_qs_switch", Values, "io,di-ul4-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw4_state_qf_switch"] = new Param("lsw4_state_qf_switch", Values, "io,di-ul4-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw4_state_tc_switch"] = new Param("lsw4_state_tc_switch", Values, "io,di-ul4-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw4_state_spare_switch"] = new Param("lsw4_state_spare_switch", Values, "io,di-ul4-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw4_alarm_short_fault"] = new Param("lsw4_alarm_short_fault", Values, "io,di-ul4-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw4_alarm_circuit_fault"] = new Param("lsw4_alarm_circuit_fault", Values, "io,di-ul4-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw4_alarm_600v_lost_power"] = new Param("lsw4_alarm_600v_lost_power", Values, "io,di-ul4-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["lsw4_state"] = new Param("lsw4_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw4_state_qs_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw4_state_qf_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw4_alarm_short_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw4_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw4_alarm_600v_lost_power"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw4_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw4_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw4_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw4_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw4_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw4_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw4_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw4_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw5_state_qs_switch"] = new Param("lsw5_state_qs_switch", Values, "io,di-ul5-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw5_state_qf_switch"] = new Param("lsw5_state_qf_switch", Values, "io,di-ul5-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw5_state_tc_switch"] = new Param("lsw5_state_tc_switch", Values, "io,di-ul5-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw5_state_spare_switch"] = new Param("lsw5_state_spare_switch", Values, "io,di-ul5-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw5_alarm_short_fault"] = new Param("lsw5_alarm_short_fault", Values, "io,di-ul5-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw5_alarm_circuit_fault"] = new Param("lsw5_alarm_circuit_fault", Values, "io,di-ul5-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw5_alarm_600v_lost_power"] = new Param("lsw5_alarm_600v_lost_power", Values, "io,di-ul5-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["lsw5_state"] = new Param("lsw5_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw5_state_qs_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw5_state_qf_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw5_alarm_short_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw5_alarm_circuit_fault"], ParamState.Unknown),
                    new ParamCombination(Params["lsw5_alarm_600v_lost_power"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw5_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw5_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw5_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw5_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw5_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw5_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw5_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw5_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw9_state_qs_switch"] = new Param("lsw9_state_qs_switch", Values, "io,di-zap-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw9_state_qf_switch"] = new Param("lsw9_state_qf_switch", Values, "io,di-zap-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw9_state_tc_switch"] = new Param("lsw9_state_tc_switch", Values, "io,di-zap-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw9_alarm_circuit_fault"] = new Param("lsw9_alarm_circuit_fault", Values, "io,di-zap-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["lsw9_state"] = new Param("lsw9_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw9_state_qs_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw9_state_qf_switch"], ParamState.Unknown),
                    new ParamCombination(Params["lsw9_alarm_circuit_fault"], ParamState.Unknown),
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw9_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw9_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw9_alarm_circuit_fault"], ParamState.Idle),
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw9_alarm_circuit_fault"], ParamState.Idle),
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["general_state_ol_switch"] = new Param("general_state_ol_switch", Values, "io,di-ol-908", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_state_ol_tc_switch"] = new Param("general_state_ol_tc_switch", Values, "io,di-ol-916", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_alarm_ol_switch_fault"] = new Param("general_alarm_ol_switch_fault", Values, "io,di-ol-910", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["general_alarm_ol_circuit_fault"] = new Param("general_alarm_ol_circuit_fault", Values, "io,di-ol-912", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["general_state_sn_automation"] = new Param("general_state_sn_automation", Values, "io,di-sn-238", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_state_sn_leadin1"] = new Param("general_state_sn_leadin1", Values, "io,di-sn-242", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_state_sn_leadin2"] = new Param("general_state_sn_leadin2", Values, "io,di-sn-244", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_alarm_sn_24v_lost_power"] = new Param("general_alarm_sn_24v_lost_power", Values, "io,di-sn-339", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["general_alarm_fire_alarm"] = new Param("general_alarm_fire_alarm", Values, "io,di-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["general_alarm_intrusion_alarm"] = new Param("general_alarm_intrusion_alarm", Values, "io,di-113", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.B, "1") }));

            Params["common_group1_state"] = new Param("common_group1_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state"], ParamState.Unknown),
                    new ParamCombination(Params["leadin2_state"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state"], ParamState.A),
                    new ParamCombination(Params["leadin2_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state"], ParamState.B),
                    new ParamCombination(Params["leadin2_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["common_group2_state"] = new Param("common_group2_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.Unknown),
                    new ParamCombination(Params["rect2_state"], ParamState.Unknown),
                    new ParamCombination(Params["rect3_state"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.A),
                    new ParamCombination(Params["rect2_state"], ParamState.A),
                    new ParamCombination(Params["rect3_state"], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.B),
                    new ParamCombination(Params["rect2_state"], ParamState.A),
                    new ParamCombination(Params["rect3_state"], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.A),
                    new ParamCombination(Params["rect2_state"], ParamState.B),
                    new ParamCombination(Params["rect3_state"], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.A),
                    new ParamCombination(Params["rect2_state"], ParamState.A),
                    new ParamCombination(Params["rect3_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.A),
                    new ParamCombination(Params["rect2_state"], ParamState.B),
                    new ParamCombination(Params["rect3_state"], ParamState.B)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.B),
                    new ParamCombination(Params["rect2_state"], ParamState.A),
                    new ParamCombination(Params["rect3_state"], ParamState.B)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.B),
                    new ParamCombination(Params["rect2_state"], ParamState.B),
                    new ParamCombination(Params["rect3_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["common_group3_state"] = new Param("common_group3_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.Unknown),
                    new ParamCombination(Params["lsw2_state"], ParamState.Unknown),
                    new ParamCombination(Params["lsw3_state"], ParamState.Unknown),
                    new ParamCombination(Params["lsw4_state"], ParamState.Unknown),
                    new ParamCombination(Params["lsw5_state"], ParamState.Unknown),
                    new ParamCombination(Params["lsw9_state"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.B),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.B),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.B),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.B),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.B),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["common_group4_state"] = new Param("common_group4_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["general_state_sn_automation"], ParamState.Unknown),
                    new ParamCombination(Params["general_alarm_sn_24v_lost_power"], ParamState.Unknown),
                    new ParamCombination(Params["general_alarm_fire_alarm"], ParamState.Unknown),
                    new ParamCombination(Params["general_alarm_intrusion_alarm"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["general_state_sn_automation"], ParamState.A),
                    new ParamCombination(Params["general_alarm_sn_24v_lost_power"], ParamState.Idle),
                    new ParamCombination(Params["general_alarm_fire_alarm"], ParamState.Idle),
                    new ParamCombination(Params["general_alarm_intrusion_alarm"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["general_alarm_sn_24v_lost_power"], ParamState.Idle),
                    new ParamCombination(Params["general_alarm_fire_alarm"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["common_state"] = new Param("common_state", new List<ParamRelation> 
            {
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["common_group1_state"], ParamState.Unknown),
                    new ParamCombination(Params["common_group2_state"], ParamState.Unknown),
                    new ParamCombination(Params["common_group3_state"], ParamState.Unknown),
                    new ParamCombination(Params["common_group4_state"], ParamState.Unknown)
                }, ParamState.Unknown),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["common_group1_state"], ParamState.A),
                    new ParamCombination(Params["common_group2_state"], ParamState.A),
                    new ParamCombination(Params["common_group3_state"], ParamState.A),
                    new ParamCombination(Params["common_group4_state"], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group1_state"], ParamState.C)
                }, ParamState.C),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group2_state"], ParamState.C)
                }, ParamState.C),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group3_state"], ParamState.C)
                }, ParamState.C),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group4_state"], ParamState.C)
                }, ParamState.C),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group2_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group3_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.B)
            });

            Params["leadin1_instant_current"] = new Param("leadin1_instant_current", Values, "io,leadin1_instant_current");
            Params["leadin1_total_active_energy"] = new Param("leadin1_total_active_energy", Values, "io,leadin1_total_active_energy");
            Params["leadin1_total_eplus_energy"] = new Param("leadin1_total_eplus_energy", Values, "io,leadin1_total_eplus_energy");
            Params["leadin1_total_eminus_energy"] = new Param("leadin1_total_eminus_energy", Values, "io,leadin1_total_eminus_energy");
            Params["leadin1_total_rplus_energy"] = new Param("leadin1_total_rplus_energy", Values, "io,leadin1_total_rplus_energy");
            Params["leadin1_total_rminus_energy"] = new Param("leadin1_total_rminus_energy", Values, "io,leadin1_total_rminus_energy");
            Params["leadin1_month_eplus_energy"] = new Param("leadin1_month_eplus_energy", Values, "io,leadin1_month_eplus_energy");
            Params["leadin1_month_eminus_energy"] = new Param("leadin1_month_eminus_energy", Values, "io,leadin1_month_eminus_energy");
            Params["leadin1_month_rplus_energy"] = new Param("leadin1_month_rplus_energy", Values, "io,leadin1_month_rplus_energy");
            Params["leadin1_month_rminus_energy"] = new Param("leadin1_month_rminus_energy", Values, "io,leadin1_month_rminus_energy");
            Params["leadin2_instant_current"] = new Param("leadin2_instant_current", Values, "io,leadin2_instant_current");
            Params["leadin2_total_active_energy"] = new Param("leadin2_total_active_energy", Values, "io,leadin2_total_active_energy");
            Params["leadin2_total_eplus_energy"] = new Param("leadin2_total_eplus_energy", Values, "io,leadin2_total_eplus_energy");
            Params["leadin2_total_eminus_energy"] = new Param("leadin2_total_eminus_energy", Values, "io,leadin2_total_eminus_energy");
            Params["leadin2_total_rplus_energy"] = new Param("leadin2_total_rplus_energy", Values, "io,leadin2_total_rplus_energy");
            Params["leadin2_total_rminus_energy"] = new Param("leadin2_total_rminus_energy", Values, "io,leadin2_total_rminus_energy");
            Params["leadin2_month_eplus_energy"] = new Param("leadin2_month_eplus_energy", Values, "io,leadin2_month_eplus_energy");
            Params["leadin2_month_eminus_energy"] = new Param("leadin2_month_eminus_energy", Values, "io,leadin2_month_eminus_energy");
            Params["leadin2_month_rplus_energy"] = new Param("leadin2_month_rplus_energy", Values, "io,leadin2_month_rplus_energy");
            Params["leadin2_month_rminus_energy"] = new Param("leadin2_month_rminus_energy", Values, "io,leadin2_month_rminus_energy");
            Params["ol_instant_current"] = new Param("ol_instant_current", Values, "io,ol_instant_current");
            Params["ol_total_active_energy"] = new Param("ol_total_active_energy", Values, "io,ol_total_active_energy");
            Params["ol_total_eplus_energy"] = new Param("ol_total_eplus_energy", Values, "io,ol_total_eplus_energy");
            Params["ol_total_eminus_energy"] = new Param("ol_total_eminus_energy", Values, "io,ol_total_eminus_energy");
            Params["ol_total_rplus_energy"] = new Param("ol_total_rplus_energy", Values, "io,ol_total_rplus_energy");
            Params["ol_total_rminus_energy"] = new Param("ol_total_rminus_energy", Values, "io,ol_total_rminus_energy");
            Params["ol_month_eplus_energy"] = new Param("ol_month_eplus_energy", Values, "io,ol_month_eplus_energy");
            Params["ol_month_eminus_energy"] = new Param("ol_month_eminus_energy", Values, "io,ol_month_eminus_energy");
            Params["ol_month_rplus_energy"] = new Param("ol_month_rplus_energy", Values, "io,ol_month_rplus_energy");
            Params["ol_month_rminus_energy"] = new Param("ol_month_rminus_energy", Values, "io,ol_month_rminus_energy");

            Channels.Add(new ProducerChannel("io", "di-rab-910"));
            Channels.Add(new ProducerChannel("io", "di-rab-908"));
            Channels.Add(new ProducerChannel("io", "di-rab-916"));
            Channels.Add(new ProducerChannel("io", "di-rab-912"));
            Channels.Add(new ProducerChannel("io", "di-tsn1-ts71"));
            Channels.Add(new ProducerChannel("io", "di-tn1-918"));
            Channels.Add(new ProducerChannel("io", "di-tn1-920"));
            Channels.Add(new ProducerChannel("io", "di-rez-902"));
            Channels.Add(new ProducerChannel("io", "di-rez-900"));
            Channels.Add(new ProducerChannel("io", "di-rez-906"));
            Channels.Add(new ProducerChannel("io", "di-rez-904"));
            Channels.Add(new ProducerChannel("io", "di-tsn2-ts71"));
            Channels.Add(new ProducerChannel("io", "di-tn2-918"));
            Channels.Add(new ProducerChannel("io", "di-tn2-741"));
            Channels.Add(new ProducerChannel("io", "di-v1-86"));
            Channels.Add(new ProducerChannel("io", "di-v1-111"));
            Channels.Add(new ProducerChannel("io", "di-v1-106"));
            Channels.Add(new ProducerChannel("io", "di-v1-67"));
            Channels.Add(new ProducerChannel("io", "di-ru1-708"));
            Channels.Add(new ProducerChannel("io", "di-ru1-710"));
            Channels.Add(new ProducerChannel("io", "di-ru1-712"));
            Channels.Add(new ProducerChannel("io", "di-ka1-n02"));
            Channels.Add(new ProducerChannel("io", "di-pa1-1003"));
            Channels.Add(new ProducerChannel("io", "di-pa1-911"));
            Channels.Add(new ProducerChannel("io", "di-v2-86"));
            Channels.Add(new ProducerChannel("io", "di-v2-111"));
            Channels.Add(new ProducerChannel("io", "di-v2-106"));
            Channels.Add(new ProducerChannel("io", "di-v2-67"));
            Channels.Add(new ProducerChannel("io", "di-ru2-708"));
            Channels.Add(new ProducerChannel("io", "di-ru2-710"));
            Channels.Add(new ProducerChannel("io", "di-ru2-712"));
            Channels.Add(new ProducerChannel("io", "di-ka2-n02"));
            Channels.Add(new ProducerChannel("io", "di-pa2-1003"));
            Channels.Add(new ProducerChannel("io", "di-pa2-911"));
            Channels.Add(new ProducerChannel("io", "di-v3-86"));
            Channels.Add(new ProducerChannel("io", "di-v3-111"));
            Channels.Add(new ProducerChannel("io", "di-v3-106"));
            Channels.Add(new ProducerChannel("io", "di-v3-67"));
            Channels.Add(new ProducerChannel("io", "di-ru3-708"));
            Channels.Add(new ProducerChannel("io", "di-ru3-710"));
            Channels.Add(new ProducerChannel("io", "di-ru3-712"));
            Channels.Add(new ProducerChannel("io", "di-ka3-n02"));
            Channels.Add(new ProducerChannel("io", "di-pa3-1003"));
            Channels.Add(new ProducerChannel("io", "di-pa3-911"));
            Channels.Add(new ProducerChannel("io", "di-ul1-708"));
            Channels.Add(new ProducerChannel("io", "di-ul1-710"));
            Channels.Add(new ProducerChannel("io", "di-ul1-712"));
            Channels.Add(new ProducerChannel("io", "di-ul1-714"));
            Channels.Add(new ProducerChannel("io", "di-ul1-716"));
            Channels.Add(new ProducerChannel("io", "di-ul1-719"));
            Channels.Add(new ProducerChannel("io", "di-ul1-n01"));
            Channels.Add(new ProducerChannel("io", "di-ul2-708"));
            Channels.Add(new ProducerChannel("io", "di-ul2-710"));
            Channels.Add(new ProducerChannel("io", "di-ul2-712"));
            Channels.Add(new ProducerChannel("io", "di-ul2-714"));
            Channels.Add(new ProducerChannel("io", "di-ul2-716"));
            Channels.Add(new ProducerChannel("io", "di-ul2-719"));
            Channels.Add(new ProducerChannel("io", "di-ul2-n01"));
            Channels.Add(new ProducerChannel("io", "di-ul3-708"));
            Channels.Add(new ProducerChannel("io", "di-ul3-710"));
            Channels.Add(new ProducerChannel("io", "di-ul3-712"));
            Channels.Add(new ProducerChannel("io", "di-ul3-714"));
            Channels.Add(new ProducerChannel("io", "di-ul3-716"));
            Channels.Add(new ProducerChannel("io", "di-ul3-719"));
            Channels.Add(new ProducerChannel("io", "di-ul3-n01"));
            Channels.Add(new ProducerChannel("io", "di-ul4-708"));
            Channels.Add(new ProducerChannel("io", "di-ul4-710"));
            Channels.Add(new ProducerChannel("io", "di-ul4-712"));
            Channels.Add(new ProducerChannel("io", "di-ul4-714"));
            Channels.Add(new ProducerChannel("io", "di-ul4-716"));
            Channels.Add(new ProducerChannel("io", "di-ul4-719"));
            Channels.Add(new ProducerChannel("io", "di-ul4-n01"));
            Channels.Add(new ProducerChannel("io", "di-ul5-708"));
            Channels.Add(new ProducerChannel("io", "di-ul5-710"));
            Channels.Add(new ProducerChannel("io", "di-ul5-712"));
            Channels.Add(new ProducerChannel("io", "di-ul5-714"));
            Channels.Add(new ProducerChannel("io", "di-ul5-716"));
            Channels.Add(new ProducerChannel("io", "di-ul5-719"));
            Channels.Add(new ProducerChannel("io", "di-ul5-n01"));
            Channels.Add(new ProducerChannel("io", "di-zap-708"));
            Channels.Add(new ProducerChannel("io", "di-zap-710"));
            Channels.Add(new ProducerChannel("io", "di-zap-712"));
            Channels.Add(new ProducerChannel("io", "di-zap-n01"));
            Channels.Add(new ProducerChannel("io", "di-sn-238"));
            Channels.Add(new ProducerChannel("io", "di-sn-242"));
            Channels.Add(new ProducerChannel("io", "di-sn-244"));
            Channels.Add(new ProducerChannel("io", "di-sn-339"));
            Channels.Add(new ProducerChannel("io", "di-111"));
            Channels.Add(new ProducerChannel("io", "di-113"));
            Channels.Add(new ProducerChannel("io", "di-ol-910"));
            Channels.Add(new ProducerChannel("io", "di-ol-908"));
            Channels.Add(new ProducerChannel("io", "di-ol-916"));
            Channels.Add(new ProducerChannel("io", "di-ol-912"));
            Channels.Add(new ProducerChannel("io", "leadin1_instant_current"));
            Channels.Add(new ProducerChannel("io", "leadin1_total_active_energy"));
            Channels.Add(new ProducerChannel("io", "leadin1_total_eplus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin1_total_eminus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin1_total_rplus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin1_total_rminus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin1_month_eplus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin1_month_eminus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin1_month_rplus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin1_month_rminus_energy"));            
            Channels.Add(new ProducerChannel("io", "leadin2_instant_current"));
            Channels.Add(new ProducerChannel("io", "leadin2_total_active_energy"));
            Channels.Add(new ProducerChannel("io", "leadin2_total_eplus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin2_total_eminus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin2_total_rplus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin2_total_rminus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin2_month_eplus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin2_month_eminus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin2_month_rplus_energy"));
            Channels.Add(new ProducerChannel("io", "leadin2_month_rminus_energy"));
            Channels.Add(new ProducerChannel("io", "ol_instant_current"));
            Channels.Add(new ProducerChannel("io", "ol_total_active_energy"));
            Channels.Add(new ProducerChannel("io", "ol_total_eplus_energy"));
            Channels.Add(new ProducerChannel("io", "ol_total_eminus_energy"));
            Channels.Add(new ProducerChannel("io", "ol_total_rplus_energy"));
            Channels.Add(new ProducerChannel("io", "ol_total_rminus_energy"));
            Channels.Add(new ProducerChannel("io", "ol_month_eplus_energy"));
            Channels.Add(new ProducerChannel("io", "ol_month_eminus_energy"));
            Channels.Add(new ProducerChannel("io", "ol_month_rplus_energy"));
            Channels.Add(new ProducerChannel("io", "ol_month_rminus_energy"));            
        }
    }
}

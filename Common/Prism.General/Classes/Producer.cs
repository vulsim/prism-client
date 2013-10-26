using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ZeroMQ;
using Newtonsoft.Json;

namespace Prism.General.Automation
{
    public class ProducerChannel
    {
        [JsonProperty("group")]
        public string Group;

        [JsonProperty("channel")]
        public string Channel;

        public ProducerChannel(string group, string channel)
        {
            this.Group = group;
            this.Channel = channel;
        }
    }

    public class ProducerChannelValue
    {
        [JsonProperty("group")]
        public string Group;

        [JsonProperty("channel")]
        public string Channel;

        [JsonProperty("value")]
        public string Value;

        public ProducerChannelValue(string group, string channel, string value)
        {
            this.Group = group;
            this.Channel = channel;
            this.Value = value;
        }
    }

    class PrismZmqCommonPubPacket
    {
        [JsonProperty("group")]
        public string Group;

        [JsonProperty("channel")]
        public string Channel;

        [JsonProperty("value")]
        public string Value;
    }

    class PrismZmqCommonReqPacket
    {
        [JsonProperty("req")]
        public string Req;

        public PrismZmqCommonReqPacket(string req)
        {
            this.Req = req;
        }
    }

    class PrismZmqCommonRepPacket
    {
        [JsonProperty("rep")]
        public string Rep;

        [JsonProperty("error")]
        public string Error;
    }

    class PrismZmqGroupListReqPacket : PrismZmqCommonReqPacket
    {
        public PrismZmqGroupListReqPacket()
            : base("glist")
        {
        }
    }

    class PrismZmqChannelListReqPacket : PrismZmqCommonReqPacket
    {
        [JsonProperty("group")]
        public string Group;

        public PrismZmqChannelListReqPacket(string group)
            : base("clist")
        {
            this.Group = group;
        }

        public PrismZmqChannelListReqPacket()
            : this(null)
        {

        }
    }

    class PrismZmqReadChannelReqPacket : PrismZmqCommonReqPacket
    {
        [JsonProperty("group")]
        public string Group;

        [JsonProperty("channel")]
        public string Channel;

        public PrismZmqReadChannelReqPacket(string group, string channel) : base("cread")
        {
            this.Group = group;
            this.Channel = channel;
        }

        public PrismZmqReadChannelReqPacket(ProducerChannel channel)
            : base("cread")
        {
            this.Group = channel.Group;
            this.Channel = channel.Channel;
        }
    }

    class PrismZmqWriteChannelReqPacket : PrismZmqCommonReqPacket
    {
        [JsonProperty("group")]
        public string Group;

        [JsonProperty("channel")]
        public string Channel;

        [JsonProperty("value")]
        public string Value;

        public PrismZmqWriteChannelReqPacket(string group, string channel, string value)
            : base("cwrite")
        {
            this.Group = group;
            this.Channel = channel;
            this.Value = value;
        }

        public PrismZmqWriteChannelReqPacket(ProducerChannelValue value)
            : base("cwrite")
        {
            this.Group = value.Group;
            this.Channel = value.Channel;
            this.Value = value.Value;
        }
    }

    class PrismZmqGroupListRepPacket : PrismZmqCommonRepPacket
    {
        [JsonProperty("groups")]
        public List<string> Groups;
    }

    class PrismZmqChannelListRepPacket : PrismZmqCommonRepPacket
    {
        [JsonProperty("channels")]
        public List<ProducerChannel> Channels;
    }

    class PrismZmqReadChannelRepPacket : PrismZmqCommonRepPacket
    {
        [JsonProperty("group")]
        public string Group;

        [JsonProperty("channel")]
        public string Channel;

        [JsonProperty("value")]
        public string Value;
    }

    class PrismZmqWriteChannelRepPacket : PrismZmqCommonRepPacket
    {
        [JsonProperty("group")]
        public string Group;

        [JsonProperty("channel")]
        public string Channel;

        [JsonProperty("value")]
        public string Value;
    }

    public class ProducerSettings
    {
        public string ReqAddr;
        public string SubAddr;
        public List<ProducerChannel> Channels;

        public ProducerSettings()
        {
            Channels = new List<ProducerChannel>();
        }
    }

    public class Producer
    {
        private class PrismReqQueueItem
        {
            public enum PrismRequestMethod
            {      
                GroupList = 1,
                ChannelList = 2,
                ReadChannel = 3,
                WriteChannel = 4
            }

            public PrismRequestMethod Method;
            public string Json;
            public object ReturnCb;

            public PrismReqQueueItem(PrismRequestMethod method, string json, object returnCb)
            {
                this.Method = method;
                this.Json = json;
                this.ReturnCb = returnCb;
            }
        }

        private class PrismReqRepContainer
        {
            public PrismReqQueueItem.PrismRequestMethod Method;
            public object ReturnCb;
            public string ReqJson;
            public string RepJson;

            public PrismReqRepContainer(PrismReqQueueItem.PrismRequestMethod method, string reqJson, string repJson, object returnCb)
            {
                this.Method = method;
                this.ReturnCb = returnCb;
                this.ReqJson = reqJson;
                this.RepJson = repJson;
            }           
        }

        private ZmqContext zmqContext = null;

        private ZmqSocket zmqReqSocket = null;        
        private Thread zmqReqWorker = null;
        private ManualResetEvent zmqReqWorkerResumeEvent = null;
        
        private ZmqSocket zmqSubSocket = null;
        private Thread zmqSubWorker = null;        

        private Queue<PrismReqQueueItem> zmqReqQueue = null;
        private ProducerSettings settings = null;

        public delegate void ChannelValueEventHandler(object sender, ProducerChannelValue value);
        public delegate void ChannelResetEventHandler(object sender, ProducerChannel channel);
        public event ChannelValueEventHandler ChannelValueEvent;
        public event ChannelResetEventHandler ChannelResetEvent;

        public delegate void GetGroupListCallback(string error, List<string> groups);
        public delegate void GetChannelListCallback(string error, List<ProducerChannel> channels);
        public delegate void ReadChannelValueCallback(string error, ProducerChannelValue value);
        public delegate void WriteChannelValueCallback(string error, ProducerChannelValue value);

        public Producer(ProducerSettings settings)
        {
            this.settings = settings;

            zmqContext = ZmqContext.Create();

            zmqSubSocket = zmqContext.CreateSocket(SocketType.SUB);
            zmqSubSocket.Connect(this.settings.SubAddr);

            foreach (ProducerChannel channel in this.settings.Channels)
            {
                zmqSubSocket.Subscribe(Encoding.UTF8.GetBytes(String.Format("{0},{1}", channel.Group, channel.Channel)));
            }

            zmqSubWorker = new Thread(delegate() {
                while (zmqSubWorker.ThreadState != ThreadState.Aborted && zmqSubWorker.ThreadState != ThreadState.AbortRequested) {                    
                    string data = zmqSubSocket.Receive(Encoding.UTF8);

                    string action = "";
                    string json = "";
                    
                    try {
                        string shortData = data.Substring(data.IndexOf(" ") + 1);
                        action = shortData.Substring(0, shortData.IndexOf(" "));
                        json = shortData.Substring(shortData.IndexOf(" ") + 1);
                    } catch (SystemException e) {
                        continue;
                    }

                    if (action.Equals("pub"))
                    {
                        if (ChannelValueEvent != null)
                        {
                            ThreadPool.QueueUserWorkItem(delegate(object target)
                            {
                                string pubJson = (string)target;

                                try
                                {
                                    PrismZmqCommonPubPacket pubPacket = JsonConvert.DeserializeObject<PrismZmqCommonPubPacket>(pubJson);
                                    ChannelValueEvent(this, new ProducerChannelValue(pubPacket.Group, pubPacket.Channel, pubPacket.Value));
                                }
                                catch (SystemException e)
                                {

                                }
                            }, json);
                        }
                    }
                    else if (action.Equals("upub"))
                    {
                        if (ChannelResetEvent != null)
                        {
                            ThreadPool.QueueUserWorkItem(delegate(object target)
                            {
                                string upubJson = (string)target;

                                try
                                {
                                    PrismZmqCommonPubPacket upubPacket = JsonConvert.DeserializeObject<PrismZmqCommonPubPacket>(upubJson);
                                    ChannelResetEvent(this, new ProducerChannel(upubPacket.Group, upubPacket.Channel));
                                }
                                catch (SystemException e)
                                {

                                }
                            }, json);
                        }
                    }
                }            
            });

            zmqReqQueue = new Queue<PrismReqQueueItem>();
            zmqReqSocket = zmqContext.CreateSocket(SocketType.REQ);
            zmqReqSocket.Connect(this.settings.ReqAddr);

            zmqReqWorkerResumeEvent = new ManualResetEvent(false);
            zmqReqWorker = new Thread(delegate() {
                while (zmqReqWorker.ThreadState != ThreadState.AbortRequested && zmqReqWorker.ThreadState != ThreadState.AbortRequested) {
                    if (zmqReqQueue.Count == 0) {
                        zmqReqWorkerResumeEvent.Reset();
                        zmqReqWorkerResumeEvent.WaitOne();
                    } else {
                        try {
                            PrismReqQueueItem reqQueueItem = zmqReqQueue.Dequeue();

                            zmqReqSocket.Send(reqQueueItem.Json, Encoding.UTF8);
                            string repJson = zmqReqSocket.Receive(Encoding.UTF8);

                            ThreadPool.QueueUserWorkItem(delegate(object target)
                            {
                                PrismReqRepContainer data = (PrismReqRepContainer)target;

                                if (target == null) {
                                    return;
                                }

                                switch (data.Method) {

                                    case PrismReqQueueItem.PrismRequestMethod.GroupList:
                                        {
                                            GetGroupListCallback cb = (GetGroupListCallback)data.ReturnCb;

                                            try {                                                
                                                PrismZmqGroupListRepPacket packet = JsonConvert.DeserializeObject<PrismZmqGroupListRepPacket>(data.RepJson);

                                                if (packet.Error != null) {
                                                    cb(packet.Error, null);
                                                } else {
                                                    cb(null, packet.Groups);
                                                }
                                            } catch (SystemException e) {
                                                cb(e.ToString(), null);
                                            }
                                            
                                            break;
                                        }

                                    case PrismReqQueueItem.PrismRequestMethod.ChannelList:
                                        {
                                            GetChannelListCallback cb = (GetChannelListCallback)data.ReturnCb;

                                            try {
                                                PrismZmqChannelListRepPacket packet = JsonConvert.DeserializeObject<PrismZmqChannelListRepPacket>(data.RepJson);

                                                if (packet.Error != null) {
                                                    cb(packet.Error, null);
                                                } else {
                                                    cb(null, packet.Channels);
                                                }
                                            } catch (SystemException e) {
                                                cb(e.ToString(), null);
                                            }
                                            break;
                                        }

                                    case PrismReqQueueItem.PrismRequestMethod.ReadChannel:
                                        {
                                            ReadChannelValueCallback cb = (ReadChannelValueCallback)data.ReturnCb;
                                            
                                            try {
                                                PrismZmqReadChannelRepPacket packet = JsonConvert.DeserializeObject<PrismZmqReadChannelRepPacket>(data.RepJson);

                                                if (packet.Error != null) {
                                                    cb(packet.Error, null);
                                                } else {
                                                    cb(null, new ProducerChannelValue(packet.Group, packet.Channel, packet.Value));
                                                }
                                            } catch (SystemException e) {
                                                cb(e.ToString(), null);
                                            }
                                            break;
                                        }

                                    case PrismReqQueueItem.PrismRequestMethod.WriteChannel:
                                        {
                                            WriteChannelValueCallback cb = (WriteChannelValueCallback)data.ReturnCb;

                                            try {
                                                PrismZmqWriteChannelRepPacket packet = JsonConvert.DeserializeObject<PrismZmqWriteChannelRepPacket>(data.RepJson);

                                                if (packet.Error != null) {
                                                    cb(packet.Error, null);
                                                } else {
                                                    cb(null, new ProducerChannelValue(packet.Group, packet.Channel, packet.Value));
                                                }
                                            } catch (SystemException e) {
                                                cb(e.ToString(), null);
                                            }
                                            break;
                                        }
                                }

                            }, new PrismReqRepContainer(reqQueueItem.Method, reqQueueItem.Json, repJson, reqQueueItem.ReturnCb));
                        } catch (SystemException e) {

                        }
                    }
                }            
            });
        }

        ~Producer()
        {
            zmqReqWorker.Abort();
            zmqReqWorker.Join();
            zmqReqSocket.Dispose();

            zmqSubWorker.Abort();
            zmqSubWorker.Join();
            zmqSubSocket.Dispose();

            zmqContext.Dispose();
        }

        public void Start()
        {
            zmqSubWorker.Start();
            zmqReqWorker.Start();
        }

        public void GetGroupList(GetGroupListCallback cb)
        {
            zmqReqQueue.Enqueue(new PrismReqQueueItem(PrismReqQueueItem.PrismRequestMethod.GroupList, JsonConvert.SerializeObject(new PrismZmqGroupListReqPacket()), cb));
            zmqReqWorkerResumeEvent.Set();
        }

        public void GetChannelList(string group, GetChannelListCallback cb)
        {
            zmqReqQueue.Enqueue(new PrismReqQueueItem(PrismReqQueueItem.PrismRequestMethod.ChannelList, JsonConvert.SerializeObject(new PrismZmqChannelListReqPacket(group)), cb));
            zmqReqWorkerResumeEvent.Set();
        }

        public void ReadChannelValue(ProducerChannel channel, ReadChannelValueCallback cb)
        {
            zmqReqQueue.Enqueue(new PrismReqQueueItem(PrismReqQueueItem.PrismRequestMethod.ReadChannel, JsonConvert.SerializeObject(new PrismZmqReadChannelReqPacket(channel)), cb));
            zmqReqWorkerResumeEvent.Set();
        }

        public void WriteChannelValue(ProducerChannelValue value, WriteChannelValueCallback cb)
        {
            zmqReqQueue.Enqueue(new PrismReqQueueItem(PrismReqQueueItem.PrismRequestMethod.WriteChannel, JsonConvert.SerializeObject(new PrismZmqWriteChannelReqPacket(value)), cb));
            zmqReqWorkerResumeEvent.Set();
        }
    }
}

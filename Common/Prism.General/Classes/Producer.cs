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

        public PrismZmqReadChannelReqPacket(string group, string channel)
            : base("cread")
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
        public delegate void ChannelValueEventHandler(object sender, ProducerChannelValue value);
        public delegate void ChannelResetEventHandler(object sender, ProducerChannel channel);

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

        private class PrismSubWorker
        {
            public event ChannelValueEventHandler ChannelValueEvent;
            public event ChannelResetEventHandler ChannelResetEvent;

            private Thread Worker;

            private ZmqSocket CreateSocket(ZmqContext context, string endpoint, List<ProducerChannel> channels)
            {
                ZmqSocket Socket = context.CreateSocket(SocketType.SUB);
                Socket.Linger = new TimeSpan(0, 0, 5);
                Socket.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 100);
                Socket.ReconnectIntervalMax = new TimeSpan(0, 0, 2);
                Socket.ReceiveTimeout = new TimeSpan(0, 0, 11);
                Socket.SendTimeout = new TimeSpan(0, 0, 4);
                Socket.Connect(endpoint);

                foreach (ProducerChannel channel in channels)
                {
                    Socket.Subscribe(Encoding.UTF8.GetBytes(String.Format("{0},{1}", channel.Group, channel.Channel)));
                }

                return Socket;
            }

            public PrismSubWorker(ZmqContext context, string endpoint, List<ProducerChannel> channels)
            {
                Worker = new Thread(delegate()
                {
                    ZmqSocket Socket = this.CreateSocket(context, endpoint, channels);

                    while (Worker.ThreadState != ThreadState.Aborted && Worker.ThreadState != ThreadState.AbortRequested)
                    {
                        string data = null;
                        string action = "";
                        string json = "";

                        try
                        {
                            data = Socket.Receive(Encoding.UTF8);
                        }
                        catch (SystemException e)
                        {
                            continue;
                        }
                        catch (ZmqSocketException e)
                        {
                            Socket.Dispose();
                            Socket = this.CreateSocket(context, endpoint, channels);
                        }

                        try
                        {
                            string shortData = data.Substring(data.IndexOf(" ") + 1);
                            action = shortData.Substring(0, shortData.IndexOf(" "));
                            json = shortData.Substring(shortData.IndexOf(" ") + 1);
                        }
                        catch (SystemException e)
                        {
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
                    Socket.Dispose();
                });
            }

            public void ProcessStart()
            {
                if (Worker != null)
                {
                    Worker.Start();
                }
            }

            public void ProcessAbort()
            {
                if (Worker != null)
                {
                    Worker.Abort();
                    Worker.Join();
                }
            }
        }

        private class PrismReqWorker
        {
            public bool IsProcess { get { return ActiveState; } }
            
            private bool ActiveState = false;
            private Thread Worker;
            private ManualResetEvent ResumeEvent;

            private ZmqSocket CreateSocket(ZmqContext context, string endpoint)
            {
                ZmqSocket Socket = context.CreateSocket(SocketType.REQ);
                Socket.Linger = new TimeSpan(0, 0, 5);
                Socket.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 100);
                Socket.ReconnectIntervalMax = new TimeSpan(0, 0, 2);
                Socket.ReceiveTimeout = new TimeSpan(0, 0, 11);
                Socket.SendTimeout = new TimeSpan(0, 0, 4);
                Socket.Connect(endpoint);
                return Socket;
            }

            public PrismReqWorker(ZmqContext context, string endpoint, Queue<PrismReqQueueItem> queue)
            {
                ResumeEvent = new ManualResetEvent(false);
                Worker = new Thread(delegate()
                {
                    ZmqSocket Socket = this.CreateSocket(context, endpoint);

                    while (Worker.ThreadState != ThreadState.AbortRequested && Worker.ThreadState != ThreadState.AbortRequested)
                    {
                        if (queue.Count == 0)
                        {
                            ActiveState = false;
                            ResumeEvent.Reset();
                            ResumeEvent.WaitOne();
                        }
                        else
                        {
                            ActiveState = true;
                            PrismReqQueueItem reqQueueItem = queue.Dequeue();

                            if (reqQueueItem != null)
                            {
                                string repJson = null;

                                try
                                {
                                    Socket.Send(reqQueueItem.Json, Encoding.UTF8).ToString();
                                    repJson = Socket.Receive(Encoding.UTF8);
                                }
                                catch (SystemException e)
                                {

                                }
                                catch (ZmqSocketException e)
                                {
                                    Socket.Dispose();
                                    Socket = this.CreateSocket(context, endpoint);
                                }

                                ThreadPool.QueueUserWorkItem(delegate(object target)
                                {
                                    PrismReqRepContainer data = (PrismReqRepContainer)target;
                                    
                                    switch (data.Method)
                                    {
                                        case PrismReqQueueItem.PrismRequestMethod.GroupList:
                                            {
                                                GetGroupListCallback cb = (GetGroupListCallback)data.ReturnCb;

                                                try
                                                {
                                                    PrismZmqGroupListRepPacket packet = JsonConvert.DeserializeObject<PrismZmqGroupListRepPacket>(data.RepJson);

                                                    if (packet.Error != null)
                                                    {
                                                        cb(packet.Error, null);
                                                    }
                                                    else
                                                    {
                                                        cb(null, packet.Groups);
                                                    }
                                                }
                                                catch (SystemException e)
                                                {
                                                    cb(e.ToString(), null);
                                                }
                                                break;
                                            }

                                        case PrismReqQueueItem.PrismRequestMethod.ChannelList:
                                            {
                                                GetChannelListCallback cb = (GetChannelListCallback)data.ReturnCb;

                                                try
                                                {
                                                    PrismZmqChannelListRepPacket packet = JsonConvert.DeserializeObject<PrismZmqChannelListRepPacket>(data.RepJson);

                                                    if (packet.Error != null)
                                                    {
                                                        cb(packet.Error, null);
                                                    }
                                                    else
                                                    {
                                                        cb(null, packet.Channels);
                                                    }
                                                }
                                                catch (SystemException e)
                                                {
                                                    cb(e.ToString(), null);
                                                }
                                                break;
                                            }

                                        case PrismReqQueueItem.PrismRequestMethod.ReadChannel:
                                            {
                                                ReadChannelValueCallback cb = (ReadChannelValueCallback)data.ReturnCb;

                                                try
                                                {
                                                    PrismZmqReadChannelRepPacket packet = JsonConvert.DeserializeObject<PrismZmqReadChannelRepPacket>(data.RepJson);

                                                    if (packet.Error != null)
                                                    {
                                                        cb(packet.Error, null);
                                                    }
                                                    else
                                                    {
                                                        cb(null, new ProducerChannelValue(packet.Group, packet.Channel, packet.Value));
                                                    }
                                                }
                                                catch (SystemException e)
                                                {
                                                    cb(e.ToString(), null);
                                                }
                                                break;
                                            }

                                        case PrismReqQueueItem.PrismRequestMethod.WriteChannel:
                                            {
                                                WriteChannelValueCallback cb = (WriteChannelValueCallback)data.ReturnCb;

                                                try
                                                {
                                                    PrismZmqWriteChannelRepPacket packet = JsonConvert.DeserializeObject<PrismZmqWriteChannelRepPacket>(data.RepJson);

                                                    if (packet.Error != null)
                                                    {
                                                        cb(packet.Error, null);
                                                    }
                                                    else
                                                    {
                                                        cb(null, new ProducerChannelValue(packet.Group, packet.Channel, packet.Value));
                                                    }
                                                }
                                                catch (SystemException e)
                                                {
                                                    cb(e.ToString(), null);
                                                }
                                                break;
                                            }
                                        default:
                                            throw new SystemException();
                                    }
                                }, new PrismReqRepContainer(reqQueueItem.Method, reqQueueItem.Json, repJson, reqQueueItem.ReturnCb));
                            }                            
                        }
                    }
                    Socket.Dispose();
                });
            }

            public void ProcessStart()
            {
                if (Worker != null)
                {
                    Worker.Start();
                }
            }

            public void ProcessAbort()
            {
                if (Worker != null)
                {
                    Worker.Abort();
                    Worker.Join();
                }
            }

            public void ProcessQueue()
            {
                if (ResumeEvent != null)
                {
                    ResumeEvent.Set();
                }
            }
        }

        private ZmqContext zmqContext = null;
        private PrismSubWorker zmqSubWorker = null;
        private List<PrismReqWorker> zmqReqWorkerPool = new List<PrismReqWorker>();

        private Queue<PrismReqQueueItem> zmqReqQueue = new Queue<PrismReqQueueItem>();
        private ProducerSettings settings = null;
        
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
            zmqSubWorker = new PrismSubWorker(zmqContext, this.settings.SubAddr, this.settings.Channels);
            zmqSubWorker.ChannelValueEvent += ChannelValue;
            zmqSubWorker.ChannelResetEvent += ChannelReset;

            //for (var i = 0; i < 5; i++)
            {
                zmqReqWorkerPool.Add(new PrismReqWorker(zmqContext, this.settings.ReqAddr, zmqReqQueue));
            }
        }

        ~Producer()
        {
            zmqSubWorker.ChannelValueEvent -= ChannelValue;
            zmqSubWorker.ChannelResetEvent -= ChannelReset;
            zmqSubWorker.ProcessAbort();

            foreach (var zmqReqWorker in zmqReqWorkerPool)
            {
                zmqReqWorker.ProcessAbort();
            }

            zmqContext.Dispose();
        }

        private void ChannelValue(object sender, ProducerChannelValue value)
        {
            if (ChannelValueEvent != null)
            {
                ChannelValueEvent(this, value);
            }
        }

        private void ChannelReset(object sender, ProducerChannel channel)
        {
            if (ChannelResetEvent != null)
            {
                ChannelResetEvent(this, channel);
            }
        }

        private void RouteProcessEnqueue()
        {
            foreach (var zmqReqWorker in zmqReqWorkerPool)
            {
                if (!zmqReqWorker.IsProcess)
                {
                    zmqReqWorker.ProcessQueue();
                    break;
                }
            }
        }

        public void Start()
        {
            zmqSubWorker.ProcessStart();

            foreach (var zmqReqWorker in zmqReqWorkerPool)
            {
                zmqReqWorker.ProcessStart();
            }
        }

        public void GetGroupList(GetGroupListCallback cb)
        {
            try
            {
                zmqReqQueue.Enqueue(new PrismReqQueueItem(PrismReqQueueItem.PrismRequestMethod.GroupList, JsonConvert.SerializeObject(new PrismZmqGroupListReqPacket()), cb));
                this.RouteProcessEnqueue();
            }
            catch (SystemException e)
            {
                cb(e.ToString(), null);
            }            
        }

        public void GetChannelList(string group, GetChannelListCallback cb)
        {
            try
            {
                zmqReqQueue.Enqueue(new PrismReqQueueItem(PrismReqQueueItem.PrismRequestMethod.ChannelList, JsonConvert.SerializeObject(new PrismZmqChannelListReqPacket(group)), cb));
                this.RouteProcessEnqueue();
            }
            catch (SystemException e)
            {
                cb(e.ToString(), null);
            }            
        }

        public void ReadChannelValue(ProducerChannel channel, ReadChannelValueCallback cb)
        {
            try
            {
                zmqReqQueue.Enqueue(new PrismReqQueueItem(PrismReqQueueItem.PrismRequestMethod.ReadChannel, JsonConvert.SerializeObject(new PrismZmqReadChannelReqPacket(channel)), cb));
                this.RouteProcessEnqueue();
            }
            catch (SystemException e)
            {
                cb(e.ToString(), null);
            }            
        }

        public void WriteChannelValue(ProducerChannelValue value, WriteChannelValueCallback cb)
        {
            try
            {
                zmqReqQueue.Enqueue(new PrismReqQueueItem(PrismReqQueueItem.PrismRequestMethod.WriteChannel, JsonConvert.SerializeObject(new PrismZmqWriteChannelReqPacket(value)), cb));
                this.RouteProcessEnqueue();
            }
            catch (SystemException e)
            {
                cb(e.ToString(), null);
            }            
        }
    }
}

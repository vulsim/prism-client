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

    public class PrismSubscribe
    {
        public delegate void ChannelValueEventHandler(object sender, ProducerChannelValue value);
        public delegate void ChannelResetEventHandler(object sender, ProducerChannel channel);

        public event ChannelValueEventHandler ChannelValueEvent;
        public event ChannelResetEventHandler ChannelResetEvent;

        private Thread Worker;
        private ZmqSocket Socket = null;
        private bool StopRequest = false;

        private ZmqSocket CreateSocket(ZmqContext context, string endpoint, List<string> groups)
        {
            ZmqSocket Socket = context.CreateSocket(SocketType.SUB);

            Socket.Linger = new TimeSpan(0, 0, 5);
            Socket.ReconnectInterval = new TimeSpan(0, 0, 1);
            Socket.ReconnectIntervalMax = new TimeSpan(0, 0, 5);

            Socket.Connect(endpoint);

            foreach (string group in groups)
            {
                Socket.Subscribe(Encoding.UTF8.GetBytes(group));
            }

            return Socket;
        }

        public PrismSubscribe(ZmqContext context, string endpoint, List<string> groups)
        {
            Worker = new Thread(delegate()
            {
                while (Worker.IsAlive && !StopRequest)
                {
                    if (Socket == null)
                    {
                        try
                        {
                            Socket = this.CreateSocket(context, endpoint, groups);
                        }
                        catch (Exception e1)
                        {
                            try
                            {
                                Socket.Close();
                            }
                            catch (Exception e2)
                            {
                            }

                            Socket = null;

                            Thread.Sleep(1000);
                            continue;
                        }                        
                    }

                    string data = null;
                    string action = "";
                    string json = "";

                    try
                    {
                        data = Socket.Receive(Encoding.UTF8);
                    }
                    catch (SystemException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                        continue;
                    }
                    catch (ZmqSocketException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                        Socket = null;
                        continue;
                    }

                    try
                    {
                        string shortData = data.Substring(data.IndexOf(" ") + 1);
                        action = shortData.Substring(0, shortData.IndexOf(" "));
                        json = shortData.Substring(shortData.IndexOf(" ") + 1);
                    }
                    catch (SystemException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
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
                                    System.Diagnostics.Debug.WriteLine(e.ToString());
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
                                    System.Diagnostics.Debug.WriteLine(e.ToString());
                                }
                            }, json);
                        }
                    }
                }

                try
                {
                    Socket.Close();
                }
                catch (Exception e)
                {
                }

                Socket = null;
            });
        }

        public void Start()
        {
            if (Worker != null)
            {
                Worker.Start();
            }
        }

        public void Abort()
        {
            if (Worker != null && Worker.ThreadState != ThreadState.Unstarted)
            {
                StopRequest = true;
                Worker.Join();
                Worker = null;
            }
        }
    }

    public class PrismRequest
    {
        public class PrismRequestItem
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
            public object DummyData;
            public object ReturnCb;

            public PrismRequestItem(PrismRequestMethod method, string json, object dummyData, object returnCb)
            {
                this.Method = method;
                this.Json = json;
                this.DummyData = dummyData;
                this.ReturnCb = returnCb;
            }
        }

        public delegate void GetGroupListCallback(string error, List<string> groups);
        public delegate void GetChannelListCallback(string error, List<ProducerChannel> channels);
        public delegate void ReadChannelValueCallback(string error, ProducerChannelValue value);
        public delegate void WriteChannelValueCallback(string error, ProducerChannelValue value);

        public bool IsActive { get { return ActiveState; } }
        private bool ActiveState = false;
        private Thread Worker;
        private ManualResetEvent ResumeEvent;
        private ManualResetEvent PauseEvent;
        private bool OnPause = false;
        private bool StopRequest = false;
        private ZmqSocket Socket = null;

        private ZmqSocket CreateSocket(ZmqContext context, string endpoint)
        {
            ZmqSocket Socket = context.CreateSocket(SocketType.REQ);

            Socket.Linger = new TimeSpan(0, 0, 5);
            Socket.ReconnectInterval = new TimeSpan(0, 0, 1);
            Socket.ReconnectIntervalMax = new TimeSpan(0, 0, 5);

            Socket.Connect(endpoint);
            return Socket;
        }

        public PrismRequest(ZmqContext context, string endpoint, Queue<PrismRequestItem> queue)
        {
            ResumeEvent = new ManualResetEvent(false);
            PauseEvent = new ManualResetEvent(false);
            Worker = new Thread(delegate()
            {
                while (Worker.IsAlive && !StopRequest)
                {
                    if (Socket == null)
                    {
                        try
                        {
                            Socket = this.CreateSocket(context, endpoint);
                        }
                        catch (Exception e1)
                        {
                            try
                            {
                                Socket.Close();
                            }
                            catch (Exception e2)
                            {
                            }

                            Socket = null;

                            Thread.Sleep(1000);
                            continue;
                        }
                    }

                    if (OnPause)
                    {
                        PauseEvent.Reset();
                        PauseEvent.WaitOne();
                        OnPause = false;
                    }

                    if (queue.Count == 0)
                    {
                        ActiveState = false;
                        ResumeEvent.Reset();
                        ResumeEvent.WaitOne();
                    }
                    else
                    {
                        ActiveState = true;
                        PrismRequestItem requestItem = queue.Dequeue();

                        if (requestItem != null)
                        {
                            string repJson = null;

                            try
                            {
                                Socket.Send(requestItem.Json, Encoding.UTF8).ToString();
                                repJson = Socket.Receive(Encoding.UTF8);
                            }
                            catch (SystemException e)
                            {
                                System.Diagnostics.Debug.WriteLine(e.ToString());
                            }
                            catch (ZmqSocketException e)
                            {
                                System.Diagnostics.Debug.WriteLine(e.ToString());
                                Socket = null;
                            }

                            switch (requestItem.Method)
                            {
                                case PrismRequestItem.PrismRequestMethod.GroupList:
                                    {
                                        GetGroupListCallback cb = (GetGroupListCallback)requestItem.ReturnCb;

                                        try
                                        {
                                            PrismZmqGroupListRepPacket packet = JsonConvert.DeserializeObject<PrismZmqGroupListRepPacket>(repJson);

                                            if (packet.Error != null)
                                            {
                                                cb(packet.Error, new List<string>());
                                            }
                                            else
                                            {
                                                cb(null, packet.Groups);
                                            }
                                        }
                                        catch (SystemException e)
                                        {
                                            System.Diagnostics.Debug.WriteLine(e.ToString());
                                            cb(e.ToString(), new List<string>());
                                        }
                                        break;
                                    }

                                case PrismRequestItem.PrismRequestMethod.ChannelList:
                                    {
                                        GetChannelListCallback cb = (GetChannelListCallback)requestItem.ReturnCb;

                                        try
                                        {
                                            PrismZmqChannelListRepPacket packet = JsonConvert.DeserializeObject<PrismZmqChannelListRepPacket>(repJson);

                                            if (packet.Error != null)
                                            {
                                                cb(packet.Error, new List<ProducerChannel>());
                                            }
                                            else
                                            {
                                                cb(null, packet.Channels);
                                            }
                                        }
                                        catch (SystemException e)
                                        {
                                            System.Diagnostics.Debug.WriteLine(e.ToString());
                                            cb(e.ToString(), new List<ProducerChannel>());
                                        }
                                        break;
                                    }

                                case PrismRequestItem.PrismRequestMethod.ReadChannel:
                                    {
                                        ReadChannelValueCallback cb = (ReadChannelValueCallback)requestItem.ReturnCb;
                                        ProducerChannel channel = (ProducerChannel)requestItem.DummyData;

                                        try
                                        {
                                            PrismZmqReadChannelRepPacket packet = JsonConvert.DeserializeObject<PrismZmqReadChannelRepPacket>(repJson);

                                            if (packet.Error != null)
                                            {
                                                cb(packet.Error, new ProducerChannelValue(channel.Group, channel.Channel, null));
                                            }
                                            else
                                            {
                                                cb(null, new ProducerChannelValue(packet.Group, packet.Channel, packet.Value));
                                            }
                                        }
                                        catch (SystemException e)
                                        {
                                            System.Diagnostics.Debug.WriteLine(e.ToString());
                                            cb(e.ToString(), new ProducerChannelValue(channel.Group, channel.Channel, null));
                                        }
                                        break;
                                    }

                                case PrismRequestItem.PrismRequestMethod.WriteChannel:
                                    {
                                        WriteChannelValueCallback cb = (WriteChannelValueCallback)requestItem.ReturnCb;
                                        ProducerChannelValue value = (ProducerChannelValue)requestItem.DummyData;

                                        try
                                        {
                                            PrismZmqWriteChannelRepPacket packet = JsonConvert.DeserializeObject<PrismZmqWriteChannelRepPacket>(repJson);

                                            if (packet.Error != null)
                                            {
                                                cb(packet.Error, new ProducerChannelValue(value.Group, value.Channel, null));
                                            }
                                            else
                                            {
                                                cb(null, new ProducerChannelValue(packet.Group, packet.Channel, packet.Value));
                                            }
                                        }
                                        catch (SystemException e)
                                        {
                                            System.Diagnostics.Debug.WriteLine(e.ToString());
                                            cb(e.ToString(), new ProducerChannelValue(value.Group, value.Channel, null));
                                        }
                                        break;
                                    }
                                default:
                                    throw new SystemException();
                            }
                        }
                    }
                }
                
                try
                {
                    Socket.Close();
                }
                catch (Exception e)
                {
                }

                Socket = null;
            });
        }

        public void Start()
        {
            if (Worker != null)
            {
                Worker.Start();
            }
        }

        public void Abort()
        {
            if (Worker != null && Worker.ThreadState != ThreadState.Unstarted)
            {
                StopRequest = true;
                
                ResumeEvent.Set();
                PauseEvent.Set();

                Worker.Join();
                Worker = null;
            }
        }

        public void Pause()
        {
            OnPause = true;
        }

        public void Resume()
        {
            if (PauseEvent != null)
            {
                PauseEvent.Set();
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

    public class PrismSubscribeProducer
    {
        private ZmqContext Context = null;
        private PrismSubscribe SubscribeWorker = null;

        public event PrismSubscribe.ChannelValueEventHandler ChannelValueEvent;
        public event PrismSubscribe.ChannelResetEventHandler ChannelResetEvent;

        public PrismSubscribeProducer(string endpoint, List<string> groups)
        {
            Context = ZmqContext.Create();
            SubscribeWorker = new PrismSubscribe(Context, endpoint, groups);
            SubscribeWorker.ChannelValueEvent += ChannelValueEventHandler;
            SubscribeWorker.ChannelResetEvent += ChannelResetEventHandler;
            SubscribeWorker.Start();
        }

        public void Terminate()
        {
            if (SubscribeWorker != null)
            {
                PrismSubscribe subscribeWorker = SubscribeWorker;

                SubscribeWorker = null;

                subscribeWorker.ChannelValueEvent -= ChannelValueEventHandler;
                subscribeWorker.ChannelResetEvent -= ChannelResetEventHandler;
                subscribeWorker.Abort();

                Context.Terminate();
                Context = null;                
            }            
        }

        private void ChannelValueEventHandler(object sender, ProducerChannelValue value)
        {
            if (ChannelValueEvent != null)
            {
                ChannelValueEvent(this, value);
            }
        }

        private void ChannelResetEventHandler(object sender, ProducerChannel channel)
        {
            if (ChannelResetEvent != null)
            {
                ChannelResetEvent(this, channel);
            }
        }
    }

    public class PrismRequestProducer
    {
        private ZmqContext Context = null;
        private List<PrismRequest> RequestWorkers = null;
        private Queue<PrismRequest.PrismRequestItem> RequestQueue = null;

        public PrismRequestProducer(List<string> endpoints)
        {
            Context = ZmqContext.Create();
            RequestWorkers = new List<PrismRequest>();
            RequestQueue = new Queue<PrismRequest.PrismRequestItem>();

            foreach (string endpoint in endpoints)
            {
                PrismRequest requestWorker = new PrismRequest(Context, endpoint, RequestQueue);
                requestWorker.Start();
                RequestWorkers.Add(requestWorker);
            }
        }

        public void Terminate()
        {
            if (RequestWorkers != null)
            {
                List<PrismRequest> requestWorkers = RequestWorkers;
                                
                RequestWorkers = null;

                foreach (PrismRequest requestWorker in requestWorkers)
                {
                    requestWorker.Abort();
                }

                requestWorkers.Clear();
                Context.Terminate();
                Context = null;

                while (RequestQueue.Count > 0)
                {
                    PrismRequest.PrismRequestItem requestItem = RequestQueue.Dequeue();

                    if (requestItem != null)
                    {
                        switch (requestItem.Method)
                        {
                            case PrismRequest.PrismRequestItem.PrismRequestMethod.GroupList:
                                {
                                    PrismRequest.GetGroupListCallback cb = (PrismRequest.GetGroupListCallback)requestItem.ReturnCb;
                                    cb("Terminating PrismRequestProducer", new List<string>());
                                    break;
                                }

                            case PrismRequest.PrismRequestItem.PrismRequestMethod.ChannelList:
                                {
                                    PrismRequest.GetChannelListCallback cb = (PrismRequest.GetChannelListCallback)requestItem.ReturnCb;
                                    cb("Terminating PrismRequestProducer", new List<ProducerChannel>());
                                    break;
                                }

                            case PrismRequest.PrismRequestItem.PrismRequestMethod.ReadChannel:
                                {
                                    PrismRequest.ReadChannelValueCallback cb = (PrismRequest.ReadChannelValueCallback)requestItem.ReturnCb;
                                    ProducerChannel channel = (ProducerChannel)requestItem.DummyData;
                                    cb("Terminating PrismRequestProducer", new ProducerChannelValue(channel.Group, channel.Channel, null));
                                    break;
                                }

                            case PrismRequest.PrismRequestItem.PrismRequestMethod.WriteChannel:
                                {
                                    PrismRequest.WriteChannelValueCallback cb = (PrismRequest.WriteChannelValueCallback)requestItem.ReturnCb;
                                    ProducerChannelValue value = (ProducerChannelValue)requestItem.DummyData;
                                    cb("Terminating PrismRequestProducer", new ProducerChannelValue(value.Group, value.Channel, null));
                                    break;
                                }
                            default:
                                throw new SystemException();
                        }
                    }
                }
            }
        }

        private void RouteProcessQueue()
        {
            foreach (PrismRequest requestWorker in RequestWorkers)
            {
                if (!requestWorker.IsActive)
                {
                    requestWorker.ProcessQueue();
                    return;
                }
            }
        }

        public void Pause()
        {
            if (RequestWorkers != null)
            {
                foreach (PrismRequest requestWorker in RequestWorkers)
                {
                    requestWorker.Pause();
                }
            }
        }

        public void Resume()
        {
            if (RequestWorkers != null)
            {
                foreach (PrismRequest requestWorker in RequestWorkers)
                {
                    requestWorker.Resume();
                }
            }
        }

        public void GetGroupList(PrismRequest.GetGroupListCallback cb)
        {
            try
            {
                RequestQueue.Enqueue(new PrismRequest.PrismRequestItem(PrismRequest.PrismRequestItem.PrismRequestMethod.GroupList, JsonConvert.SerializeObject(new PrismZmqGroupListReqPacket()), null, cb));
                RouteProcessQueue();
            }
            catch (SystemException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                cb(e.ToString(), null);
            }
        }

        public void GetChannelList(string group, PrismRequest.GetChannelListCallback cb)
        {
            try
            {
                RequestQueue.Enqueue(new PrismRequest.PrismRequestItem(PrismRequest.PrismRequestItem.PrismRequestMethod.ChannelList, JsonConvert.SerializeObject(new PrismZmqChannelListReqPacket(group)), group, cb));
                RouteProcessQueue();
            }
            catch (SystemException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                cb(e.ToString(), null);
            }
        }

        public void ReadChannelValue(ProducerChannel channel, PrismRequest.ReadChannelValueCallback cb)
        {
            try
            {
                RequestQueue.Enqueue(new PrismRequest.PrismRequestItem(PrismRequest.PrismRequestItem.PrismRequestMethod.ReadChannel, JsonConvert.SerializeObject(new PrismZmqReadChannelReqPacket(channel)), channel, cb));
                RouteProcessQueue();
            }
            catch (SystemException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                cb(e.ToString(), null);
            }
        }

        public void WriteChannelValue(ProducerChannelValue value, PrismRequest.WriteChannelValueCallback cb)
        {
            try
            {
                RequestQueue.Enqueue(new PrismRequest.PrismRequestItem(PrismRequest.PrismRequestItem.PrismRequestMethod.WriteChannel, JsonConvert.SerializeObject(new PrismZmqWriteChannelReqPacket(value)), value, cb));
                RouteProcessQueue();
            }
            catch (SystemException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                cb(e.ToString(), null);
            }
        }
    }
}

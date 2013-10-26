using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Prism.General;
using Prism.General.Automation;

namespace TransportTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ProducerSettings settings = new ProducerSettings();
            settings.ReqAddr = "tcp://127.0.0.1:1233";
            settings.SubAddr = "tcp://127.0.0.1:1234";
            settings.Channels.Add(new ProducerChannel("io", "test-001"));

            Producer producer = new Producer(settings);

            producer.ChannelValueEvent += new Producer.ChannelValueEventHandler(delegate(object sender, ProducerChannelValue value)
            {
                Console.WriteLine("P: {0},{1} {2}", value.Group, value.Channel, value.Value);
            });
            producer.Start();

            while (true)
            {
                producer.ReadChannelValue(new ProducerChannel("io", "test-002"), delegate(string error, ProducerChannelValue value)
                {
                    if (error != null)
                    {
                        Console.WriteLine("RC: {0}", error.ToString());
                    }
                    else
                    {
                        Console.WriteLine("RC: {0},{1} {2}", value.Group, value.Channel, value.Value);
                    }
                });
                Thread.Sleep(2000);
            }
        }
    }
}

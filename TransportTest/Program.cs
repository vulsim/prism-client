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
            settings.ReqAddr = "tcp://192.168.45.2:1233";
            settings.SubAddr = "tcp://192.168.45.2:1234";
            settings.Channels.Add(new ProducerChannel("io", "ol_total_active_energy"));

            Producer producer = new Producer(settings);

            producer.ChannelValueEvent += new Producer.ChannelValueEventHandler(delegate(object sender, ProducerChannelValue value)
            {
                Console.WriteLine("P: {0},{1} {2}", value.Group, value.Channel, value.Value);
            });
            producer.Start();

            bool interrupted = false;

            Console.CancelKeyPress += delegate 
            { 
                interrupted = true; 
            };

            /*producer.WriteChannelValue(new ProducerChannelValue("auto", "lsw1-ast-ctrl", "M"), delegate(string error, ProducerChannelValue value)
            {
                if (error != null)
                {
                    Console.WriteLine("WC: {0}", error.ToString());
                }
                else
                {
                    Console.WriteLine("WC: {0},{1} {2}", value.Group, value.Channel, value.Value);
                }
            });
            return;*/

            while (!interrupted)
            {
                producer.ReadChannelValue(new ProducerChannel("io", "ol_total_active_energy"), delegate(string error, ProducerChannelValue value)
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

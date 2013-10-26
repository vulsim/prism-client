using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Prism.General
{
    public delegate void MainThreadQueueItemHandler();

    public class MainThread
    {
        private static MainThread Instance = new MainThread();
        private Thread InvokeThread;
        private Queue<MainThreadQueueItemHandler> InvokeQueue;
        private ManualResetEvent ContinueQueueEvent;

        public static void EnqueueTask(MainThreadQueueItemHandler queueItemHandler)
        {
            if (Instance == null)
            {
                return;
            }

            Instance.Enqueue(queueItemHandler);
        }

        public MainThread()
        {
            InvokeQueue = new Queue<MainThreadQueueItemHandler>();
            ContinueQueueEvent = new ManualResetEvent(false);

            InvokeThread = new Thread(delegate()
            {
                while (InvokeThread.ThreadState != ThreadState.Aborted)
                {
                    if (InvokeQueue.Count > 0)
                    {
                        try
                        {
                            MainThreadQueueItemHandler handler = InvokeQueue.Dequeue();
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, handler);
                        }
                        catch (SystemException e)
                        {

                        }
                    }
                    else
                    {
                        ContinueQueueEvent.WaitOne();
                    }
                    ContinueQueueEvent.Reset();
                }
            });
            InvokeThread.SetApartmentState(ApartmentState.STA);
            InvokeThread.Start();
        }

        ~MainThread()
        {
            InvokeThread.Abort();
            ContinueQueueEvent.Set();
            InvokeThread.Join();
        }

        public void Enqueue(MainThreadQueueItemHandler queueItemHandler)
        {
            try
            {
                InvokeQueue.Enqueue(queueItemHandler);
                ContinueQueueEvent.Set();
            }
            catch (SystemException e)
            {

            }
        }
    }
}

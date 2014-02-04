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
        private static Dispatcher CurrentDispatcher;
        public static Dispatcher Dispatcher { get { return CurrentDispatcher; } }

        public static void Initialize(Dispatcher dispatcher)
        {
            if (CurrentDispatcher == null)
            {
                CurrentDispatcher = dispatcher;
            }
        }

        public static void EnqueueTask(MainThreadQueueItemHandler queueItemHandler)
        {
            if (CurrentDispatcher == null)
            {
                return;
            }

            CurrentDispatcher.BeginInvoke(new Action(queueItemHandler), DispatcherPriority.Normal, null);
        }       
    }
}

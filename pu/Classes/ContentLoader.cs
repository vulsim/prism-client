using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Prism.Classes
{
    public delegate void ContentLoaderHandler(object target);
    public delegate void ContentLoaderProgressHandler(double progress, string description);

    class ContentLoaderItem
    {
        public string Description;
        public ContentLoaderHandler Handler;
        public object Target;

        public ContentLoaderItem(string description, ContentLoaderHandler handler, object target)
        {
            this.Description = description;
            this.Handler = handler;
            this.Target = target;
        }
    }    

    class ContentLoader
    {
        private Thread ContentLoaderThread;
        private Queue<ContentLoaderItem> LoaderQueue;
        private ManualResetEvent ContinueLoaderThreadEvent;

        public ContentLoader(ContentLoaderProgressHandler progress)
        {
            LoaderQueue = new Queue<ContentLoaderItem>();
            ContinueLoaderThreadEvent = new ManualResetEvent(false);

            ContentLoaderThread = new Thread(delegate() {
                double itemIndex = 0;

                while (ContentLoaderThread.ThreadState != ThreadState.Aborted)
                {
                    if (LoaderQueue.Count > 0)
                    {                        
                        ContentLoaderItem item = LoaderQueue.Dequeue();
                        progress((++itemIndex) / (itemIndex + LoaderQueue.Count), item.Description);
                        item.Handler(item.Target);
                    }
                    else 
                    {
                        ContinueLoaderThreadEvent.WaitOne();
                    }
                    ContinueLoaderThreadEvent.Reset();
                }              
            });
            ContentLoaderThread.Start();
        }

        ~ContentLoader()
        {
            try {
                ContentLoaderThread.Abort();
                ContentLoaderThread.Join();
            } catch (SystemException e) { 

            }            
        }

        public void Enqueue(string description, ContentLoaderHandler handler, object target)
        {
            LoaderQueue.Enqueue(new ContentLoaderItem(description, handler, target));
            ContinueLoaderThreadEvent.Set();
        }
    }
}

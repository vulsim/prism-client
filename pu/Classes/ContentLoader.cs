using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Prism.Classes
{
    public delegate void ContentLoaderHandler();
    public delegate void ContentLoaderProgressHandler(uint stage, string description);

    class ContentLoaderItem
    {
        public string Description;
        public ContentLoaderHandler Handler;

        public ContentLoaderItem(string description, ContentLoaderHandler handler)
        {
            this.Description = description;
            this.Handler = handler;
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
                uint itemIndex = 0;

                while (ContentLoaderThread.ThreadState != ThreadState.Aborted)
                {
                    if (LoaderQueue.Count > 0)
                    {
                        ContentLoaderItem item = LoaderQueue.Dequeue();
                        progress(itemIndex++, item.Description);
                        item.Handler();
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

        public void Enqueue(string description, ContentLoaderHandler handler)
        {
            LoaderQueue.Enqueue(new ContentLoaderItem(description, handler));
            ContinueLoaderThreadEvent.Set();
        }
    }
}

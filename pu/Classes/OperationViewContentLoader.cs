using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirstFloor.ModernUI.Windows;
using Prism.Views;
using Prism.General;
using Prism.Classes;

namespace Prism.Classes
{
    public class OperationViewContentLoader : IContentLoader
    {
        public Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken)
        {
            return Task<object>.Factory.StartNew(delegate()
            {
                ManualResetEvent continueEvent = new ManualResetEvent(false);
                UnitOperationView unitOperationView = null;

                MainThread.EnqueueTask(delegate()
                {
                    foreach (var unit in Core.Instance.Units)
                    {
                        if (unit.Uri.Equals(uri))
                        {
                            unitOperationView = new UnitOperationView(unit);
                            break;
                        }
                    }                    
                    continueEvent.Set();
                });

                continueEvent.WaitOne();
                return unitOperationView; 
            });
        }
    }
}

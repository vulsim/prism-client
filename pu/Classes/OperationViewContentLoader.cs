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
                UnitOperationView unitOperationView = null;

                foreach (var unit in Core.Instance.Units)
                {
                    if (unit.Uri.Equals(uri))
                    {
                        ManualResetEvent continueEvent = new ManualResetEvent(false);
                        MainThread.EnqueueTask(delegate()
                        {
                            unitOperationView = new UnitOperationView(unit);
                            continueEvent.Set();
                        });
                        continueEvent.WaitOne();
                        break;
                    }
                }
                return unitOperationView; 
            });
        }
    }
}

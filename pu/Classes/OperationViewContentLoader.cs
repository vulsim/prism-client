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
        private DefaultContentLoader defaultContentLoader = new DefaultContentLoader();
        private Dictionary<String, UnitOperationView> cached = new Dictionary<string,UnitOperationView>();

        public Task<object> LoadContentAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (cached.ContainsKey(uri.ToString()))
            {
                return Task<object>.Factory.StartNew(delegate()
                {
                    UnitOperationView unitOperationView = null;
                    cached.TryGetValue(uri.ToString(), out unitOperationView);
                    return unitOperationView;
                });
            }
            else
            {
                foreach (var unit in Core.Instance.Units)
                {
                    if (unit.Uri.Equals(uri))
                    {
                        return Task<object>.Factory.StartNew(delegate()
                        {
                            UnitOperationView unitOperationView = null;
                            ManualResetEvent continueEvent = new ManualResetEvent(false);
                            MainThread.EnqueueTask(delegate()
                            {
                                unitOperationView = new UnitOperationView(unit);
                                cached.Add(unit.Uri.ToString(), unitOperationView);
                                continueEvent.Set();
                            });
                            continueEvent.WaitOne();                            
                            return unitOperationView;
                        });
                    }
                }
            }
            return defaultContentLoader.LoadContentAsync(uri, cancellationToken);
        }
    }
}

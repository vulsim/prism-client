using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using FirstFloor.ModernUI.Presentation;
using Prism.Classes;
using Prism.General;

namespace Prism.Views
{
    /// <summary>
    /// Логика взаимодействия для OperationRootView.xaml
    /// </summary>
    public partial class OperationRootView : UserControl
    {
        public OperationRootView()
        {
            InitializeComponent();

            foreach (var unit in Core.Instance.Units)
            {
                Link link = new Link();
                link.DisplayName = unit.ShortName;
                link.Source = unit.Uri;
                operationTab.Links.Add(link);
            }
            operationTab.SelectedSource = Core.Instance.Units.First<Unit>().Uri;

            generalBusyProgress.Visibility = Core.Instance.IsCoreBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            Core.Instance.CoreBusyStateChangedEvent += CoreBusyStateChangedEvent;
            CoreBusyStateChangedEvent(this, Core.Instance.IsCoreBusy);
        }

        ~OperationRootView()
        {
            Core.Instance.CoreBusyStateChangedEvent -= CoreBusyStateChangedEvent;
        }

        private void CoreBusyStateChangedEvent(object sender, bool isBusy)
        {
            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                MainThread.EnqueueTask(delegate()
                {
                    generalBusyProgress.Visibility = isBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                    generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
                });                
            }, null);
        }
    }
}

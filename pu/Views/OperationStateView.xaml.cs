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
using System.Collections.ObjectModel;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using Prism.Classes;
using Prism.General;
using Prism.ViewModels;

namespace Prism.Views
{
    /// <summary>
    /// Логика взаимодействия для OperationStateView.xaml
    /// </summary>
    public partial class OperationStateView : UserControl
    {
        public List<OperationStateTileViewModel> OperationStateItems { get; set; }

        public OperationStateView()
        {
            OperationStateItems = new List<OperationStateTileViewModel>();

            InitializeComponent();
            
            generalBusyProgress.Visibility = Core.Instance.IsCoreBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            Core.Instance.CoreBusyStateChangedEvent += CoreBusyStateChangedEvent;
            CoreBusyStateChangedEvent(this, Core.Instance.IsCoreBusy);

            foreach (var unit in Core.Instance.Units)
            {
                OperationStateItems.Add(new OperationStateTileViewModel(unit));                
            }
        }

        ~OperationStateView()
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

        private void ListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (operationListBox.SelectedItem != null)
            {
                OperationStateTileViewModel selectedItem = (OperationStateTileViewModel)operationListBox.SelectedItem;
                MainWindow.Instance.ContentSource = selectedItem.Unit.Uri;
            }            
        }     
    }
}

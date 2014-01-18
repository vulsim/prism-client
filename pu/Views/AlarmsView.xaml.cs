using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using FirstFloor.ModernUI.Windows.Controls;
using Prism.Classes;
using Prism.General;
using Prism.ViewModels;

namespace Prism.Views
{
    /// <summary>
    /// Логика взаимодействия для AlarmsView.xaml
    /// </summary>
    public partial class AlarmsView : UserControl
    {
        public List<NotificationAlarm> NotificationAlarms { get; set; }

        public AlarmsView()
        {
            NotificationAlarms = new List<NotificationAlarm>();
            InitializeComponent();

            generalBusyProgress.Visibility = Core.Instance.IsCoreBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            Core.Instance.CoreBusyStateChangedEvent += CoreBusyStateChangedEvent;
            CoreBusyStateChangedEvent(this, Core.Instance.IsCoreBusy);
        }

        ~AlarmsView()
        {
            Core.Instance.CoreBusyStateChangedEvent -= CoreBusyStateChangedEvent;
        }

        private void view_Loaded(object sender, RoutedEventArgs e)
        {
            NotificationAlarms.Clear();
            NotificationAlarms.AddRange(AlarmNotificationCenter.Instance.alarms);
            dataGrid.Items.Refresh();
            AlarmNotificationCenter.Instance.GeneralAlarmsStateChangedEvent += GeneralAlarmsStateChangedEvent;
        }

        private void view_Unloaded(object sender, RoutedEventArgs e)
        {
            AlarmNotificationCenter.Instance.GeneralAlarmsStateChangedEvent -= GeneralAlarmsStateChangedEvent;
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

        void GeneralAlarmsStateChangedEvent(object sender)
        {
            NotificationAlarms.Clear();
            NotificationAlarms.AddRange(AlarmNotificationCenter.Instance.alarms);
            dataGrid.Items.Refresh();
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                NotificationAlarm selectedItem = (NotificationAlarm)dataGrid.SelectedItem;

                if (!selectedItem.Ack)
                {
                    if (ModernDialog.ShowMessage("Вы уверены, что хотите квитировать сообщение?", "Предупреждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        selectedItem.Ack = true;
                        dataGrid.Items.Refresh();
                    }                    
                }
            }
        }
    }
}

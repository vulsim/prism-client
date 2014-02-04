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
using Prism.General.Automation;
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

            generalBusyProgress.Visibility = Core.Instance.IsBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            
            Core.Instance.CoreBusyStateChangedEvent += CoreBusyStateChangedEvent;
            CoreBusyStateChangedEvent(this, Core.Instance.IsBusy);
        }

        ~AlarmsView()
        {
            Core.Instance.CoreBusyStateChangedEvent -= CoreBusyStateChangedEvent;
        }

        private void view_Loaded(object sender, RoutedEventArgs e)
        {
            NotificationAlarms.Clear();
            NotificationAlarms.AddRange(AlarmNotificationCenter.Instance.Alarms);
            NotificationAlarms.Sort(NotificationAlarmsCompare);
            alarmsListBox.Items.Refresh();
            AlarmNotificationCenter.Instance.GeneralAlarmsStateChangedEvent += GeneralAlarmsStateChangedEvent;
        }

        private void view_Unloaded(object sender, RoutedEventArgs e)
        {
            AlarmNotificationCenter.Instance.GeneralAlarmsStateChangedEvent -= GeneralAlarmsStateChangedEvent;
        }

        private void CoreBusyStateChangedEvent(object sender, bool isBusy)
        {
            MainThread.EnqueueTask(delegate()
            {
                generalBusyProgress.Visibility = isBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            });
        }

        void GeneralAlarmsStateChangedEvent(object sender)
        {
            NotificationAlarms.Clear();
            NotificationAlarms.AddRange(AlarmNotificationCenter.Instance.Alarms);
            NotificationAlarms.Sort(NotificationAlarmsCompare);

            MainThread.EnqueueTask(delegate()
            {
                alarmsListBox.Items.Refresh();
            });            
        }

        private void alarmsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (alarmsListBox.SelectedItem != null)
            {
                NotificationAlarm selectedItem = (NotificationAlarm)alarmsListBox.SelectedItem;

                if (!selectedItem.Ack)
                {
                    if (MessageBox.Show(String.Format("{0} [{1}, {2}]: {3}\n\nВы уверены, что хотите квитировать это сообщение?", selectedItem.Unit.FullName, selectedItem.Code, ParamStateConverter.ToString(selectedItem.State), selectedItem.Description), "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)                                        
                    {
                        selectedItem.Ack = true;
                        NotificationAlarms.Clear();
                        NotificationAlarms.AddRange(AlarmNotificationCenter.Instance.Alarms);
                        NotificationAlarms.Sort(NotificationAlarmsCompare);
                        alarmsListBox.Items.Refresh();
                    }
                }
            }

        }

        private static int NotificationAlarmsCompare(NotificationAlarm a, NotificationAlarm b)
        {
            if (a.State > b.State)
            {
                return -1;
            }
            else if (a.State < b.State)
            {
                return 1;
            }
            else
            {
                if (a.Ack && !b.Ack)
                {
                    return 1;
                }
                else if (!a.Ack && b.Ack)
                {
                    return -1;
                }
                
                return 0;
            }
        }        
    }
}

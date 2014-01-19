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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using FirstFloor.ModernUI.Windows.Controls;
using Prism.Classes;
using Prism.General;
using Prism.ViewModels;

namespace Prism.Views
{
    /// <summary>
    /// Логика взаимодействия для JournalView.xaml
    /// </summary>
    public partial class JournalView : UserControl
    {
        public class JournalEntry
        {
            public int Type { get; set; }
            public string UnitName { get; set; }
            public DateTime Date { get; set; }                       
            public string Message { get; set; }

            public JournalEntry(int type, string unitName, DateTime date, string message)
            {
                Type = type;
                UnitName = unitName;
                Date = date;
                Message = message;
            }
        }

        public List<JournalEntry> CurrentJournal { get; set; }

        public JournalView()
        {
            CurrentJournal = new List<JournalEntry>();
            InitializeComponent();

            generalBusyProgress.Visibility = Core.Instance.IsCoreBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            Core.Instance.CoreBusyStateChangedEvent += CoreBusyStateChangedEvent;
            CoreBusyStateChangedEvent(this, Core.Instance.IsCoreBusy);
        }

        private void view_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox.SelectedIndex = 0;            
        }

        private void view_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void CoreBusyStateChangedEvent(object sender, bool isBusy)
        {
            MainThread.EnqueueTask(delegate()
            {
                generalBusyProgress.Visibility = isBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            });
        }

        private string GetUnitName(string category)
        {
            Regex regex = new Regex("[0-9]+");
            Match match = regex.Match(category);

            if (match.Groups.Count == 1)
            {
                category = match.Groups[0].Value;
            }           

            foreach (Unit unit in Core.Instance.Units)
            {
                if (unit.SymbolicName.Equals(category))
                {
                    return unit.ShortName;
                }
            }

            return "";
        }

        private List<JournalEntry> GetJournalFromDate(DateTime date)
        {
            List<JournalEntry> result = new List<JournalEntry>();
 
            EventLog eventLog = new EventLog();
            eventLog.Source = Journal.EventLogSource;

            EventLogEntryCollection eventLogEntryCollection = eventLog.Entries;
            eventLog.Close();

            for (int i = 0; i < eventLogEntryCollection.Count; i++)
            {
                EventLogEntry eventLogEntry = eventLogEntryCollection[i];
                
                if (eventLogEntry.TimeGenerated > date && eventLogEntry.Source.Equals(Journal.EventLogSource))
                {
                    switch (eventLogEntry.EntryType)
                    {
                        case EventLogEntryType.Information:
                            result.Add(new JournalEntry(0, this.GetUnitName(eventLogEntry.Category), eventLogEntry.TimeGenerated, eventLogEntry.Message));
                            break;
                        case EventLogEntryType.Warning:
                            result.Add(new JournalEntry(1, this.GetUnitName(eventLogEntry.Category), eventLogEntry.TimeGenerated, eventLogEntry.Message));
                            break;
                        case EventLogEntryType.Error:
                            result.Add(new JournalEntry(2, this.GetUnitName(eventLogEntry.Category), eventLogEntry.TimeGenerated, eventLogEntry.Message));
                            break;
                    }
                    
                }
            }

            return result;
        }

        private void dataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            dataGrid.UnselectAll();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime date = DateTime.Now.AddDays(-1);

            switch (comboBox.SelectedIndex)
            {
                case 1:
                    date = DateTime.Now.AddDays(-7);
                    break;
                case 2:
                    date = DateTime.Now.AddDays(-30);
                    break;
            }

            overlay.Visibility = System.Windows.Visibility.Visible;
            progress.IsActive = true;
            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                CurrentJournal.Clear();
                CurrentJournal.AddRange(this.GetJournalFromDate(date));
                MainThread.EnqueueTask(delegate()
                {
                    dataGrid.Items.Refresh();
                    overlay.Visibility = System.Windows.Visibility.Hidden;
                    progress.IsActive = false;
                });
            }, null);
        }
    }
}

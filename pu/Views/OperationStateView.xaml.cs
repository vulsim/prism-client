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
using System.Collections.ObjectModel;
using FirstFloor.ModernUI.Presentation;
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

            foreach (var unit in Core.Instance.Units)
            {
                OperationStateItems.Add(new OperationStateTileViewModel(unit));                
            }
        }

        private void ListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (operationListBox.SelectedItem != null)
            {
                OperationStateTileViewModel selectedItem = (OperationStateTileViewModel)operationListBox.SelectedItem;

                //MessageBox.Show(MainWindow.Instance.ContentSource.OriginalString.ToString());


                MainWindow.Instance.ContentSource = new Uri("/Views/OperationRootView.xaml", UriKind.Relative);
            }            
        }     
    }
}

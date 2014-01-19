using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using Prism.General;
using Prism.Classes;

namespace Prism.Views
{
    /// <summary>
    /// Логика взаимодействия для UnitOperationView.xaml
    /// </summary>
    public partial class UnitOperationView : UserControl
    {
        private Unit Unit;
        private ICollectionView collectionView;
        private UserControl currentPresentationControl;

        public UnitOperationView(Unit unit)
        {
            InitializeComponent();

            this.Unit = unit;

            generalBusyProgress.Visibility = Core.Instance.IsCoreBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            Core.Instance.CoreBusyStateChangedEvent += CoreBusyStateChangedEvent;
            CoreBusyStateChangedEvent(this, Core.Instance.IsCoreBusy);

            unitTitle.Text = Unit.FullName.ToUpper();
            collectionView = CollectionViewSource.GetDefaultView(Unit.PresentationControls);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            operationControlList.ItemsSource = collectionView;
        }

        ~UnitOperationView()
        {
            Core.Instance.CoreBusyStateChangedEvent -= CoreBusyStateChangedEvent;
        }

        private void CoreBusyStateChangedEvent(object sender, bool isBusy)
        {
            MainThread.EnqueueTask(delegate()
            {
                generalBusyProgress.Visibility = isBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            });
        }

        private void operationControlList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentPresentationControl != null)
            {
                containerGrid.Children.Remove(currentPresentationControl);
            }

            currentPresentationControl = ((IPresentationControl)operationControlList.SelectedItem).Control;

            if (currentPresentationControl != null)
            {
                containerGrid.Children.Add(currentPresentationControl);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {           
            IPresentationControl firstPresentationControl = this.Unit.PresentationControls.First<IPresentationControl>();

            if (currentPresentationControl == null && firstPresentationControl != null)
            {
                currentPresentationControl = firstPresentationControl.Control;

                if (currentPresentationControl != null)
                {
                    containerGrid.Children.Add(currentPresentationControl);
                }
            }
        }
    }
}

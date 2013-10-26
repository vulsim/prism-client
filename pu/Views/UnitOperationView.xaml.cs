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

            collectionView = CollectionViewSource.GetDefaultView(unit.PresentationControls);
            collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            operationControlList.ItemsSource = collectionView;
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

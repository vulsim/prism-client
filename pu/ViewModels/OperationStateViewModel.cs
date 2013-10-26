using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using MahApps.Metro.Controls;
using Prism.Classes;
using Prism.General;

namespace Prism.ViewModels
{
    class OperationStateViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<PanoramaGroup> OperationGroups { get; set; }
        public List<OperationStateTileViewModel> TileGroup;

        public OperationStateViewModel()
        {
            TileGroup = new List<OperationStateTileViewModel>();

            foreach (var unit in Core.Instance.Units)
            {
                TileGroup.Add(new OperationStateTileViewModel(unit));
            }

            OperationGroups = new ObservableCollection<PanoramaGroup> { new PanoramaGroup("", CollectionViewSource.GetDefaultView(TileGroup)) };
        }
      
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}

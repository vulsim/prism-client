using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.ComponentModel;
using Prism.Classes;
using Prism.General;
using Prism.General.Automation;

namespace Prism.ViewModels
{
    public class OperationStateTileViewModel : INotifyPropertyChanged
    {
        public Unit Unit;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Index { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ParamState State { get; set; }

        public OperationStateTileViewModel(Unit unit)
        {
            Unit = unit;
            Title = Unit.ShortName;
            Description = Unit.Address;
            State = Unit.State;
            Index = Unit.SymbolicName;

            Unit.UnitStateChangedEvent += UnitStateChangedEvent;
        }

        ~OperationStateTileViewModel()
        {
            Unit.UnitStateChangedEvent -= UnitStateChangedEvent;
        }

        private void UnitStateChangedEvent(object sender, ParamState hazardValue)
        {
            State = Unit.State;
            NotifyPropertyChanged("State");
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

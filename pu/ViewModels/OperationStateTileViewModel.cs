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
    class OperationStateTileViewModel : INotifyPropertyChanged
    {
        private Unit Unit;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title { get; set; }
        public string Description { get; set; }
        public ParamState State { get; set; }

        public OperationStateTileViewModel(Unit unit)
        {
            Unit = unit;
            Title = Unit.ShortName;
            Description = Unit.Address;
            State = Unit.State;

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

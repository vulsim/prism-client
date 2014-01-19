using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Prism.General.Automation;

namespace Prism.General
{
    public delegate void UnitBusyStateChangedEventHandler(object sender, bool isBusy);
    public delegate void UnitStateChangedEventHandler(object sender, ParamState state);
    public delegate void UnitAlarmsChangedEventHandler(object sender, IEnumerable<IAlarm> alarms);
    public delegate void UnitGeneralCompleteHandler();

    public interface IUnit
    {
        bool IsOnline { get; }
        bool IsBusy { get; }
        ParamState State { get; }

        string FullName { get; }
        string ShortName { get; }
        string SymbolicName { get; }
        string Address { get; }

        IEnumerable<IAlarm> Alarms { get; }
        IEnumerable<IPresentationControl> PresentationControls { get; }

        event UnitBusyStateChangedEventHandler UnitBusyStateChangedEvent;
        event UnitStateChangedEventHandler UnitStateChangedEvent;
        event UnitAlarmsChangedEventHandler UnitAlarmsChangedEvent;

        void Initialize(UnitSettings settings, IJournal journal, UnitGeneralCompleteHandler complete);
        void Uninitialize();
        void BecomeActive(IPresentationControl presentationControl);
        void ResignActive();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Prism.General.Automation;

namespace Prism.General
{
    public delegate void UnitGeneralCompleteHandler();
    public delegate void UnitParamUpdateEventHandler(object sender, Param param);
    public delegate void UnitAlarmUpdateEventHandler(object sender, IAlarm alarm);
    public delegate void UnitStateChangedEventHandler(object sender, ParamState state);

    public interface IUnit
    {
        bool IsOnline { get; }
        ParamState State { get; }

        string FullName { get; }
        string ShortName { get; }
        string SymbolicName { get; }
        string Address { get; }

        IEnumerable<IAlarm> Alarms { get; }
        IEnumerable<IPresentationControl> PresentationControls { get; }

        event UnitParamUpdateEventHandler UnitParamUpdateEvent;
        event UnitAlarmUpdateEventHandler UnitAlarmUpdateEvent;
        event UnitStateChangedEventHandler UnitStateChangedEvent;

        void Initialize(System.Windows.Threading.Dispatcher dispatcher, UnitSettings settings, IPollManager manager, IJournal journal, UnitGeneralCompleteHandler complete);
        void Uninitialize();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Prism.General;
using Prism.General.Automation;

namespace Prism.Classes
{
    public class Unit
    {
        public IUnit Instance;
        public Uri Uri { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string SymbolicName { get; set; }
        public string Address { get; set; }
        public bool IsOnline { get { return Instance.IsOnline; } }        
        public ParamState State { get { return Instance.State; } }

        public IEnumerable<IAlarm> Alarms { get { return Instance.Alarms; } }
        public IEnumerable<IPresentationControl> PresentationControls { get { return Instance.PresentationControls; } }

        public event UnitParamUpdateEventHandler UnitParamUpdateEvent;
        public event UnitAlarmUpdateEventHandler UnitAlarmUpdateEvent;
        public event UnitStateChangedEventHandler UnitStateChangedEvent;

        public Unit(UnitSettings settings, IPollManager manager, IJournal journal, UnitGeneralCompleteHandler completion)
        {
            string filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + settings.FilePath;
            Assembly assembly = Assembly.LoadFile(filePath);

            foreach (Type t in assembly.GetTypes())
            {
                if (t.GetInterfaces().Contains(typeof(IUnit)))
                {
                    Instance = (IUnit)Activator.CreateInstance(t);
                    break;
                }
            }

            this.FullName = settings.FullName;
            this.ShortName = settings.ShortName;
            this.SymbolicName = settings.SymbolicName;
            this.Address = settings.Address;

            Instance.Initialize(MainThread.Dispatcher, settings, manager, journal, completion);

            Instance.UnitParamUpdateEvent += delegate(object sender, Param param)
            {
                if (UnitParamUpdateEvent != null)
                {
                    UnitParamUpdateEvent(this, param);
                }
            };

            Instance.UnitAlarmUpdateEvent += delegate(object sender, IAlarm alarm)
            {
                if (UnitAlarmUpdateEvent != null)
                {
                    UnitAlarmUpdateEvent(this, alarm);
                }
            };

            Instance.UnitStateChangedEvent += delegate(object sender, ParamState state)
            {
                if (UnitStateChangedEvent != null)
                {
                    UnitStateChangedEvent(this, state);
                }
            };
        }
    }
}

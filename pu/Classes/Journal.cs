using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Prism.General;
using Prism.General.Automation;

namespace Prism.Classes
{
    public class Journal : IJournal
    {
        public static string EventLogSource = "Prism SCADA G1";

        public Journal()
        {
            if (!EventLog.SourceExists(Journal.EventLogSource))
            {
                EventLog.CreateEventSource(Journal.EventLogSource, "Application");
            }            
        }

        public void Informarion(IUnit unit, int code, string message)
        {
            try
            {
                EventLog.WriteEntry(Journal.EventLogSource, String.Format("{0} [{1}]: {2}", unit.FullName, code.ToString(), message), EventLogEntryType.Information, code, Convert.ToInt16(unit.SymbolicName));
            }
            catch (SystemException e)
            {

            }
        }

        public void Warning(IUnit unit, int code, string message)
        {
            try
            {
                EventLog.WriteEntry(Journal.EventLogSource, String.Format("{0} [{1}]: {2}", unit.FullName, code.ToString(), message), EventLogEntryType.Warning, code, Convert.ToInt16(unit.SymbolicName));
            }
            catch (SystemException e)
            {

            }
        }

        public void Error(IUnit unit, int code, string message)
        {
            try
            {
                EventLog.WriteEntry(Journal.EventLogSource, String.Format("{0} [{1}]: {2}", unit.FullName, code.ToString(), message), EventLogEntryType.Error, code, Convert.ToInt16(unit.SymbolicName));
            }
            catch (SystemException e)
            {

            }
        }


        public void Log(IUnit unit, IAlarm alarm)
        {
            int code = 10000;
            string[] codeComp = alarm.Code.Split(new Char[] { '-' });

            if (codeComp.Length == 2)
            {
                code += Convert.ToInt32(codeComp[1]);
            }

            try
            {
                switch (alarm.State)
                {
                    case ParamState.Idle:
                    case ParamState.A:
                        EventLog.WriteEntry(Journal.EventLogSource, String.Format("{0} [{1}, {2}]: {3}", unit.FullName, alarm.Code, ParamStateConverter.ToString(alarm.State), alarm.Description), EventLogEntryType.Information, code, Convert.ToInt16(unit.SymbolicName));
                        break;
                    case ParamState.B:
                        EventLog.WriteEntry(Journal.EventLogSource, String.Format("{0} [{1}, {2}]: {3}", unit.FullName, alarm.Code, ParamStateConverter.ToString(alarm.State), alarm.Description), EventLogEntryType.Warning, code, Convert.ToInt16(unit.SymbolicName));
                        break;
                    case ParamState.C:
                        EventLog.WriteEntry(Journal.EventLogSource, String.Format("{0} [{1}, {2}]: {3}", unit.FullName, alarm.Code, ParamStateConverter.ToString(alarm.State), alarm.Description), EventLogEntryType.Error, code, Convert.ToInt16(unit.SymbolicName));
                        break;
                    case ParamState.Unknown:
                    default:
                        EventLog.WriteEntry(Journal.EventLogSource, String.Format("{0} [{1}, {2}]: {3}", unit.FullName, alarm.Code, ParamStateConverter.ToString(alarm.State), alarm.Description), EventLogEntryType.Information, code, Convert.ToInt16(unit.SymbolicName));
                        break;
                }
            }
            catch (SystemException e)
            {

            }

        }
    }
}

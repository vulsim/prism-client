using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.General.Automation;

namespace Prism.General
{
    public interface IJournal
    {
        void Informarion(IUnit unit, int code, string message);
        void Warning(IUnit unit, int code, string message);
        void Error(IUnit unit, int code, string message);
        void Log(IUnit unit, IAlarm alarm);        
    }
}

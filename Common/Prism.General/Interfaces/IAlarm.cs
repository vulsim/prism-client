using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prism.General.Automation
{
    public interface IAlarm
    {
        int Code { get; set; }       
        string Description { get; set; }
        ParamState State { get; set; }
    }
}

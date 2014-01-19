using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.General.Automation;

namespace Prism.Units.Classes
{
    public class Alarm : IAlarm
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public ParamState State { get; set; }

        public Alarm(string code, string description, ParamState state)
        {
            this.Code = code;
            this.Description = description;
            this.State = state;
        }
    }
}

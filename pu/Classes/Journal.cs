using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.General;

namespace Prism.Classes
{
    public class Journal : IJournal
    {
        public void Message(int category, int code, float hazardValue, string description)
        {

        }

        public void Warning(int category, int code, float hazardValue, string description)
        {

        }

        public void Error(int category, int code, float hazardValue, string description)
        {

        }
    }
}

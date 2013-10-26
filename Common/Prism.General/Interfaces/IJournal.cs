using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prism.General
{
    public interface IJournal
    {
        void Message(int category, int code, float hazardValue, string description);
        void Warning(int category, int code, float hazardValue, string description);
        void Error(int category, int code, float hazardValue, string description);
    }
}

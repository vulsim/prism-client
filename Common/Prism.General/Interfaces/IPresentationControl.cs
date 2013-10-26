using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Prism.General
{
    public enum PresentationControlCategory : uint
    {
        Category0 = 0,
        Category1 = 1,
        Category2 = 2,
        Category3 = 3,
        Category4 = 4,
        Category5 = 5,
        Category6 = 6,
        Category7 = 7,
        Category8 = 8,
        Category9 = 9
    }

    public interface IPresentationControl
    {
        PresentationControlCategory Category { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        UserControl Control { get; set; }        
    }
}

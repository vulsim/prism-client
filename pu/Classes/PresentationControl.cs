using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Prism.General;

namespace Prism.Classes
{
    public class PresentationControl : IPresentationControl
    {
        public PresentationControlCategory Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public UserControl Control { get; set; }

        public PresentationControl(PresentationControlCategory category, string title, string description, UserControl control)
        {
            this.Category = category;
            this.Title = title;
            this.Description = description;
            this.Control = control;
        }
    }
}

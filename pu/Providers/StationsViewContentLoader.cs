using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstFloor.ModernUI.Windows;
using Prism.Controls;

namespace Prism
{
    class StationsViewContentLoader : DefaultContentLoader
    {
        protected override object LoadContent(Uri uri)
        {
            return new ShockwaveFlashControl("c:\\Users\\vulsi_000\\Documents\\Projects\\GrodnoTrolleybus\\trunk\\Pickup\\Concept\\PS-5.swf");
        }
    }
}

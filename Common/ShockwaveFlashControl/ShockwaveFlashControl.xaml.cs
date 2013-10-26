using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AxShockwaveFlashObjects;

namespace Prism.Controls
{
    /// <summary>
    /// Логика взаимодействия для ShockwaveFlashControl.xaml
    /// </summary>
    public partial class ShockwaveFlashControl : UserControl
    {
        public delegate void ShockwaveFlashControlFSCommandEventHandler(object sender, string args, string command);
        
        private string sourceUri = null;
        private AxShockwaveFlashObjects.AxShockwaveFlash shockwaveFlashObject;

        public event ShockwaveFlashControlFSCommandEventHandler ShockwaveFlashControlFSCommandEvent;
        public string Uri
        {
            get { return shockwaveFlashObject.Movie; }

            set { shockwaveFlashObject.Movie = Uri; }
        }
        
        public ShockwaveFlashControl(string uri) : base()
        {
            shockwaveFlashObject = new AxShockwaveFlashObjects.AxShockwaveFlash();
            shockwaveFlashObject.FSCommand += ShockwaveFlashControl_FSCommand;
            InitializeComponent();

            shockwaveFlashHost.Child = shockwaveFlashObject;
            sourceUri = uri;
        }

        ~ShockwaveFlashControl()
        {
            shockwaveFlashObject.FSCommand -= ShockwaveFlashControl_FSCommand;
        }

        public void CallFunction(string request)
        {
            shockwaveFlashObject.CallFunction(request);
        }

        private void ShockwaveFlashControl_Loaded(object sender, RoutedEventArgs e)
        {
            shockwaveFlashObject.Menu = false;

            if (sourceUri != null)
            {
                shockwaveFlashObject.Movie = sourceUri;
                sourceUri = null;
            }
        }

        private void ShockwaveFlashControl_FSCommand(object sender, _IShockwaveFlashEvents_FSCommandEvent e)
        {
            if (ShockwaveFlashControlFSCommandEvent != null)
            {
                ShockwaveFlashControlFSCommandEvent(this, e.args, e.command);
            }
        }
    }
}

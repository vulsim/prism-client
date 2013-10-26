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
using System.Threading;
using Prism.Classes;
using Prism.General;
using System.Speech.Synthesis;

namespace Prism.Views
{
    /// <summary>
    /// Логика взаимодействия для ExploreRootView.xaml
    /// </summary>
    public partial class ExploreRootView : UserControl
    {
        private SpeechSynthesizer synthesizer;

        public ExploreRootView()
        {
            InitializeComponent();

            generalBusyProgress.Visibility = Core.Instance.IsCoreBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            Core.Instance.CoreBusyStateChangedEvent += CoreBusyStateChangedEvent;
            CoreBusyStateChangedEvent(this, Core.Instance.IsCoreBusy);

            synthesizer = new SpeechSynthesizer();
            synthesizer.SpeakAsync("Whish you where here");
        }

        ~ExploreRootView()
        {
            Core.Instance.CoreBusyStateChangedEvent -= CoreBusyStateChangedEvent;
        }

        private void CoreBusyStateChangedEvent(object sender, bool isBusy)
        {
            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                MainThread.EnqueueTask(delegate()
                {
                    generalBusyProgress.Visibility = isBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                    generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
                });                
            }, null);
        }
    }
}

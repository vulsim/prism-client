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
using System.Reflection;
using Prism.Classes;
using Prism.General;
using Prism.ViewModels;

namespace Prism.Views
{
    /// <summary>
    /// Логика взаимодействия для AboutView.xaml
    /// </summary>
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();

            var assemblyProductAttribute = ((AssemblyProductAttribute[])Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false)).Single();
            copyrightLabel.Content = "© Кафедра ИСИТ, ГрГУ им. Я.Купалы, 2014. Все права защищены.";
            versionLabel.Content = String.Format("{0}, v.{1}", assemblyProductAttribute.Product.ToString(), Assembly.GetEntryAssembly().GetName().Version.ToString());

            generalBusyProgress.Visibility = Core.Instance.IsBusy ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            generalBusyProgress.IsIndeterminate = (generalBusyProgress.Visibility == System.Windows.Visibility.Visible);
            
            Core.Instance.CoreBusyStateChangedEvent += CoreBusyStateChangedEvent;
            CoreBusyStateChangedEvent(this, Core.Instance.IsBusy);
        }

        ~AboutView()
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

        private void UpdateManagerProgressEventHandler(object sender, string stage)
        {
            MainThread.EnqueueTask(delegate()
            {
                updateLabel.Content = stage;
            });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateManager.Instance.UpdateManagerProgressEvent += UpdateManagerProgressEventHandler;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UpdateManager.Instance.UpdateManagerProgressEvent -= UpdateManagerProgressEventHandler;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateManager.Instance.CheckForUpdate();
        }
    }
}

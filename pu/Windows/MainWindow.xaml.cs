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
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Presentation;
using Prism.Classes;
using Prism.General;
using Prism.General.Automation;

namespace Prism
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public static MainWindow Instance = null;
        
        public MainWindow()
        {
            InitializeComponent();
            AppearanceManager.Current.AccentColor = Color.FromRgb(0xa4, 0xa4, 0xa4);
        }

        private void SetGeneralState(ParamState state)
        {
            switch (state)
            {
                case ParamState.C:
                    AppearanceManager.Current.AccentColor = Color.FromRgb(0xd2, 0x00, 0x00);
                    break;
                case ParamState.B:
                    AppearanceManager.Current.AccentColor = Color.FromRgb(0xf0, 0xd8, 0x00);
                    break;
                case ParamState.A:
                    AppearanceManager.Current.AccentColor = Color.FromRgb(0x69, 0xd2, 0x00);
                    break;
                default:
                    AppearanceManager.Current.AccentColor = Color.FromRgb(0xa4, 0xa4, 0xa4);
                    break;
            }
        }

        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Core.Instance.Shutdown();
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Core.Instance.CoreGeneralStateChangedEvent += delegate(object core, ParamState state)
            {
                ThreadPool.QueueUserWorkItem(delegate(object target)
                {
                    MainThread.EnqueueTask(delegate()
                    {
                        SetGeneralState(state);
                    });
                }, null);                
            };
            SetGeneralState(Core.Instance.CoreGeneralState);
        }
    }
}

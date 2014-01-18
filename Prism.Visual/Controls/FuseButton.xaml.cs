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
using System.Timers;
using Prism.General;

namespace Prism.Visual.Controls
{
    /// <summary>
    /// Логика взаимодействия для FuseButton.xaml
    /// </summary>
    public partial class FuseButton : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FuseButton), new PropertyMetadata(""));
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FuseButton));

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        private System.Timers.Timer unlockTimer;
        private bool IsUnlocked = false;
        private int UnlockedTriggeredCount = 0;

        public FuseButton()
        {
            InitializeComponent();

            unlockTimer = new System.Timers.Timer(5000);
            unlockTimer.Elapsed += UnlockTimerEvent;
        }

        ~FuseButton()
        {
            unlockTimer.Stop();
            unlockTimer.Elapsed -= UnlockTimerEvent;
        }

        private void UnlockTimerEvent(object sender, ElapsedEventArgs e)
        {
            unlockTimer.Stop();

            IsUnlocked = false;
            MainThread.EnqueueTask(delegate()
            {
                overlay.Visibility = System.Windows.Visibility.Hidden;
            });

            /*if (IsUnlocked && UnlockedTriggeredCount < 10)
            {
                UnlockedTriggeredCount++;
                MainThread.EnqueueTask(delegate()
                {
                    overlay.Visibility = (UnlockedTriggeredCount % 2 > 0) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                });
            }
            else
            {
                IsUnlocked = false;
                UnlockedTriggeredCount = 0;                
                unlockTimer.Stop();

                MainThread.EnqueueTask(delegate()
                {
                    overlay.Visibility = System.Windows.Visibility.Hidden;
                });
            }*/
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (IsUnlocked)
            {
                //if (UnlockedTriggeredCount > 1)
                {
                    IsUnlocked = false;
                    overlay.Visibility = System.Windows.Visibility.Hidden;
                    RoutedEventArgs args = new RoutedEventArgs(ClickEvent, this);
                    RaiseEvent(args);
                }
            }
            else
            {
                IsUnlocked = true;
                overlay.Visibility = System.Windows.Visibility.Visible;
                //UnlockedTriggeredCount = 0;
                unlockTimer.Start();
            }
        }
    }
}

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
    /// Логика взаимодействия для FuseImageButton.xaml
    /// </summary>
    public partial class FuseImageButton : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FuseImageButton), new PropertyMetadata(""));
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(string), typeof(FuseImageButton), new PropertyMetadata(""));
        public static readonly DependencyProperty IsUnlockedProperty = DependencyProperty.Register("IsUnlocked", typeof(bool), typeof(FuseImageButton), new PropertyMetadata(false));
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FuseImageButton));

        private System.Timers.Timer unlockTimer;

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public string ImageSource
        {
            get { return (string)this.GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }

        public bool IsUnlocked
        {
            get { return (bool)this.GetValue(IsUnlockedProperty); }
        }

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
       
        public FuseImageButton()
        {
            InitializeComponent();

            unlockTimer = new System.Timers.Timer(5000);
            unlockTimer.Elapsed += UnlockTimerEvent;
        }

        ~FuseImageButton()
        {
            unlockTimer.Stop();
            unlockTimer.Elapsed -= UnlockTimerEvent;
        }

        private void UnlockTimerEvent(object sender, ElapsedEventArgs e)
        {
            unlockTimer.Stop();
            MainThread.EnqueueTask(delegate()
            {
                this.SetValue(IsUnlockedProperty, false);
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (IsUnlocked)
            {
                this.SetValue(IsUnlockedProperty, false);

                RoutedEventArgs args = new RoutedEventArgs(ClickEvent, this);
                RaiseEvent(args);
            }
            else
            {
                this.SetValue(IsUnlockedProperty, true);
                unlockTimer.Start();
            }
        }
    }
}

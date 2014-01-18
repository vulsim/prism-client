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
using Prism.General;
using Prism.General.Automation;

namespace Prism.Visual.Controls
{
    /// <summary>
    /// Логика взаимодействия для StateTile.xaml
    /// </summary>
    public partial class StateTile : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(StateTile), new PropertyMetadata(""));
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(ParamState), typeof(StateTile), new PropertyMetadata(ParamState.Unknown));
        
        public ParamState State
        {
            get { return (ParamState)this.GetValue(StateProperty); }
            set { this.SetValue(StateProperty, value); }
        }

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public StateTile()
        {
            InitializeComponent();
        }
    }
}

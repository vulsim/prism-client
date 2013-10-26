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
using Prism.Controls;
using Prism.Units.Classes;
using Prism.General.Automation;

namespace Prism.Units.Controls
{
    /// <summary>
    /// Логика взаимодействия для LeadinControl.xaml
    /// </summary>
    public partial class LeadinControl : UserControl
    {
        private Unit Unit;
        private uint Index;

        public LeadinControl(Unit unit, uint index, String title)
        {
            InitializeComponent();

            this.Unit = unit;
            this.Index = index;
            this.titleText.Text = title;

            Unit.Processing.ProcessingChangeStateEvent += delegate(object sender)
            {
                
            };

            Unit.Processing.ProcessingUpdateEvent += delegate(object sender)
            {

            };
        }

        private void onButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void offButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

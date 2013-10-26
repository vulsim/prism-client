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
    /// Логика взаимодействия для Lsw9Control.xaml
    /// </summary>
    public partial class Lsw9Control : UserControl
    {
        private Unit Unit;
        private Param qfOnCtrlState;
        private Param qfOffCtrlState;

        public Lsw9Control(Unit unit, String title)
        {
            InitializeComponent();

            this.Unit = unit;
            this.titleText.Text = title;

            qfOnCtrlState = new Param("qf_on_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params["lsw9_state_qf_switch"], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params["lsw9_state_qs_switch"], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params["lsw9_state_tc_switch"], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params["lsw9_alarm_circuit_fault"], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });

            qfOffCtrlState = new Param("qf_off_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params["lsw9_state_qf_switch"], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params["lsw9_state_tc_switch"], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params["lsw9_alarm_circuit_fault"], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateState();
            Unit.Processing.ProcessingChangeStateEvent += delegate(object s)
            {
                MainThread.EnqueueTask(delegate()
                {
                    UpdateState();
                });
            };
        }

        private void UpdateState()
        {
            qfOnButton.IsEnabled = (qfOnCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            qfOffButton.IsEnabled = (qfOffCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
        }

        private void qfOnButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            Unit.Processing.Operate(new ProducerChannelValue("auto", "lsw9-qf-ctrl", "A"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (!value.Value.Equals("A"))
                    {
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    UpdateState();
                });
            });
        }

        private void qfOffButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            Unit.Processing.Operate(new ProducerChannelValue("auto", "lsw9-qf-ctrl", "B"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (!value.Value.Equals("B"))
                    {
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    UpdateState();
                });
            });
        }
    }
}

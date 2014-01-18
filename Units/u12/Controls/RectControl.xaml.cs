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
using Prism.General;
using Prism.Controls;
using Prism.Units.Classes;
using Prism.General.Automation;

namespace Prism.Units.Controls
{
    /// <summary>
    /// Логика взаимодействия для RectControl.xaml
    /// </summary>
    public partial class RectControl : UserControl
    {
        private Unit Unit;
        private uint Index;

        private Param paOnCtrlState;
        private Param paOffCtrlState;
        private Param kaOnCtrlState;
        private Param kaOffCtrlState;
        private Boolean lockUpdate = false;

        public RectControl(Unit unit, uint index, String title)
        {
            InitializeComponent();

            this.Unit = unit;
            this.Index = index;
            this.titleText.Text = title;

            paOnCtrlState = new Param("pa_on_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_pa_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_alarm_pa_switch_fault", Index)], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });

            paOffCtrlState = new Param("pa_off_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_pa_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_alarm_pa_switch_fault", Index)], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });

            kaOnCtrlState = new Param("ka_on_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_qf_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_qs_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_alarm_circuit_fault", Index)], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });

            kaOffCtrlState = new Param("ka_off_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_qf_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_qs_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("rect{0}_alarm_circuit_fault", Index)], ParamState.C)
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

        private void paOnButton_Click(object sender, RoutedEventArgs e)
        {
            paOnButton.IsEnabled = false;
            paOffButton.IsEnabled = false;
            kaOnButton.IsEnabled = false;
            kaOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("rect{0}-pa-ctrl", Index), "A"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("A"))
                    {
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    lockExtendedControl.IsChecked = false;
                    lockUpdate = false;
                    UpdateState();
                });
            });
        }

        private void paOffButton_Click(object sender, RoutedEventArgs e)
        {
            paOnButton.IsEnabled = false;
            paOffButton.IsEnabled = false;
            kaOnButton.IsEnabled = false;
            kaOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("rect{0}-pa-ctrl", Index), "B"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("B"))
                    {
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    lockExtendedControl.IsChecked = false;
                    lockUpdate = false;
                    UpdateState();
                });
            });
        }

        private void kaOnButton_Click(object sender, RoutedEventArgs e)
        {
            paOnButton.IsEnabled = false;
            paOffButton.IsEnabled = false;
            kaOnButton.IsEnabled = false;
            kaOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("rect{0}-ka-ctrl", Index), "A"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("A"))
                    {
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    lockExtendedControl.IsChecked = false;
                    lockUpdate = false;
                    UpdateState();
                });
            });
        }

        private void kaOffButton_Click(object sender, RoutedEventArgs e)
        {
            paOnButton.IsEnabled = false;
            paOffButton.IsEnabled = false;
            kaOnButton.IsEnabled = false;
            kaOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("rect{0}-ka-ctrl", Index), "B"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("B"))
                    {
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    lockExtendedControl.IsChecked = false;
                    lockUpdate = false;
                    UpdateState();
                });
            });
        }

        private void UpdateState()
        {
            rectStatePaTile.State = Unit.Processing.Params[String.Format("rect{0}_state_pa_switch", Index)].State;
            rectStateQfTile.State = Unit.Processing.Params[String.Format("rect{0}_state_qf_switch", Index)].State;
            rectStateQsTile.State = Unit.Processing.Params[String.Format("rect{0}_state_qs_switch", Index)].State;
            rectStateTcTile.State = Unit.Processing.Params[String.Format("rect{0}_state_tc_switch", Index)].State;
            rectAlarmCircuitTile.State = Unit.Processing.Params[String.Format("rect{0}_alarm_circuit_fault", Index)].State;
            rectAlarmGasTile.State = Unit.Processing.Params[String.Format("rect{0}_alarm_rec_gas_warn", Index)].State;
            rectAlarmOverloadTile.State = Unit.Processing.Params[String.Format("rect{0}_alarm_rec_overload", Index)].State;
            rectAlarmPaSwitchTile.State = Unit.Processing.Params[String.Format("rect{0}_alarm_pa_switch_fault", Index)].State;
            rectAlarmRecFaultTile.State = Unit.Processing.Params[String.Format("rect{0}_alarm_rec_fault", Index)].State;
            rectAlarmRpz600Tile.State = Unit.Processing.Params[String.Format("rect{0}_alarm_rec_rpz600v_fault", Index)].State;

            if (lockUpdate)
            {
                return;
            }

            paOnButton.IsEnabled = (paOnCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            paOffButton.IsEnabled = (paOffCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            kaOnButton.IsEnabled = (kaOnCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            kaOffButton.IsEnabled = (kaOnCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
        }
    }
}

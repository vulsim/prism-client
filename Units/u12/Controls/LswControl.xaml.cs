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
    /// Логика взаимодействия для LswControl.xaml
    /// </summary>
    public partial class LswControl : UserControl
    {
        private Unit Unit;
        private uint Index;

        private Param spareCtrlState;
        private Param qsOnCtrlState;
        private Param qfOnCtrlState;
        private Param spareOnCtrlState;
        private Param qsOffCtrlState;
        private Param qfOffCtrlState;
        private Param spareOffCtrlState;
        private Boolean lockUpdate = false;

        public LswControl(Unit unit, uint index, String title)
        {
            InitializeComponent();

            this.Unit = unit;
            this.Index = index;
            this.titleText.Text = title;

            spareCtrlState = new Param("spare_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_alarm_circuit_fault", Index)], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_spare_switch", Index)], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_spare_switch", Index)], ParamState.B)
                }, ParamState.B)
            });

            qsOnCtrlState = new Param("qs_on_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qs_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qf_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_spare_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_alarm_circuit_fault", Index)], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });

            qsOffCtrlState = new Param("qs_off_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qs_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qf_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_spare_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_alarm_circuit_fault", Index)], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });

            qfOnCtrlState = new Param("qf_on_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qf_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qs_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_spare_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_alarm_circuit_fault", Index)], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });

            qfOffCtrlState = new Param("qf_off_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qf_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qs_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_spare_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_alarm_circuit_fault", Index)], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });

            spareOnCtrlState = new Param("spare_on_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qf_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qs_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_spare_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_alarm_circuit_fault", Index)], ParamState.C)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.A)
            });

            spareOffCtrlState = new Param("spare_off_ctrl_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qf_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_qs_switch", Index)], ParamState.A)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_tc_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_state_spare_switch", Index)], ParamState.B)
                }, ParamState.Idle),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Unit.Processing.Params[String.Format("lsw{0}_alarm_circuit_fault", Index)], ParamState.C)
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

        private void qsOnButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-qs-ctrl", Index), "A"), delegate(string error, ProducerChannelValue value)
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

        private void qfOnButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-qf-ctrl", Index), "A"), delegate(string error, ProducerChannelValue value)
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

        private void spareOnButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-spare-ctrl", Index), "A"), delegate(string error, ProducerChannelValue value)
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

        private void qsOffButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-qs-ctrl", Index), "B"), delegate(string error, ProducerChannelValue value)
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

        private void qfOffButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-qf-ctrl", Index), "B"), delegate(string error, ProducerChannelValue value)
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

        private void spareOffButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-spare-ctrl", Index), "B"), delegate(string error, ProducerChannelValue value)
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

        private void onButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-qf-ctrl", Index), "A"), delegate(string error, ProducerChannelValue value)
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

        private void offButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-qf-ctrl", Index), "B"), delegate(string error, ProducerChannelValue value)
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

        private void mainButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-ast-ctrl", Index), "M"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("M"))
                    {
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    lockExtendedControl.IsChecked = false;
                    lockUpdate = false;
                    UpdateState();
                });
            });
        }

        private void spareButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            spareButton.IsEnabled = false;
            qsOnButton.IsEnabled = false;
            qsOffButton.IsEnabled = false;
            qfOnButton.IsEnabled = false;
            qfOffButton.IsEnabled = false;
            spareOnButton.IsEnabled = false;
            spareOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            lockUpdate = true;
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("lsw{0}-ast-ctrl", Index), "S"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {                   
                    if (error != null || value == null || !value.Value.Equals("S"))
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
            lswStateQfTile.State = Unit.Processing.Params[String.Format("lsw{0}_state_qf_switch", Index)].State;
            lswStateQsTile.State = Unit.Processing.Params[String.Format("lsw{0}_state_qs_switch", Index)].State;
            lswStateSpareTile.State = Unit.Processing.Params[String.Format("lsw{0}_state_spare_switch", Index)].State;
            lswStateTcTile.State = Unit.Processing.Params[String.Format("lsw{0}_state_tc_switch", Index)].State;
            lswAlarm600Tile.State = Unit.Processing.Params[String.Format("lsw{0}_alarm_600v_lost_power", Index)].State;
            lswAlarmCircuitTile.State = Unit.Processing.Params[String.Format("lsw{0}_alarm_circuit_fault", Index)].State;
            lswAlarmShortTile.State = Unit.Processing.Params[String.Format("lsw{0}_alarm_short_fault", Index)].State;

            if (lockUpdate)
            {
                return;
            }

            onButton.IsEnabled = (qfOnCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            offButton.IsEnabled = (qfOffCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            mainButton.IsEnabled = (spareCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            spareButton.IsEnabled = (spareCtrlState.State == ParamState.B && Unit.Processing.IsAvaliable);
            qsOnButton.IsEnabled = (qsOnCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            qfOnButton.IsEnabled = (qfOnCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            spareOnButton.IsEnabled = (spareOnCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            qsOffButton.IsEnabled = (qsOffCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            qfOffButton.IsEnabled = (qfOffCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            spareOffButton.IsEnabled = (spareOffCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);            
        }
    }
}

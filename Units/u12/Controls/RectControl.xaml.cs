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
using System.Timers;
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
        private System.Timers.Timer alertTimer;

        public RectControl(Unit unit, uint index, String title)
        {
            InitializeComponent();

            this.Unit = unit;
            this.Index = index;
            this.titleText.Text = title;

            alertTimer = new System.Timers.Timer(1000);
            alertTimer.Elapsed += AlertTimerEvent;

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

        ~RectControl()
        {
            alertTimer.Stop();
            alertTimer.Elapsed -= AlertTimerEvent;
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

        private void AlertTimerEvent(object sender, ElapsedEventArgs e)
        {
            MainThread.EnqueueTask(delegate()
            {
                alertMessageBlock.Visibility = (alertMessageBlock.Visibility == System.Windows.Visibility.Hidden) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            });
        }

        private void paOnButton_Click(object sender, RoutedEventArgs e)
        {
            paOnButton.IsEnabled = false;
            paOffButton.IsEnabled = false;
            kaOnButton.IsEnabled = false;
            kaOffButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            overlay.Visibility = System.Windows.Visibility.Visible;
            progress.IsActive = true;
            
            lockUpdate = true;
            Unit.Journal.Informarion(Unit, (int)(2000 + 10 * Index), String.Format("Телеуправление - ВА №{0}, включение...", Index));
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("rect{0}-pa-ctrl", Index), "A"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("A"))
                    {
                        Unit.Journal.Error(Unit, (int)(2000 + 10 * Index), String.Format("Телеуправление - ВА №{0}, включение не произведено либо завершилось с ошибкой.", Index));
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Unit.Journal.Informarion(Unit, (int)(2000 + 10 * Index), String.Format("Телеуправление - ВА №{0}, включение произведено.", Index));
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

            overlay.Visibility = System.Windows.Visibility.Visible;
            progress.IsActive = true;

            lockUpdate = true;
            Unit.Journal.Informarion(Unit, (int)(2001 + 10 * Index), String.Format("Телеуправление - ВА №{0}, отключение...", Index));
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("rect{0}-pa-ctrl", Index), "B"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("B"))
                    {
                        Unit.Journal.Error(Unit, (int)(2001 + 10 * Index), String.Format("Телеуправление - ВА №{0}, отключение не произведено либо завершилось с ошибкой.", Index));
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Unit.Journal.Informarion(Unit, (int)(2001 + 10 * Index), String.Format("Телеуправление - ВА №{0}, отключение произведено.", Index));
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

            overlay.Visibility = System.Windows.Visibility.Visible;
            progress.IsActive = true;

            lockUpdate = true;
            Unit.Journal.Informarion(Unit, (int)(2002 + 10 * Index), String.Format("Телеуправление - ВА №{0}, включение УРК...", Index));
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("rect{0}-ka-ctrl", Index), "A"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("A"))
                    {
                        Unit.Journal.Error(Unit, (int)(2002 + 10 * Index), String.Format("Телеуправление - ВА №{0}, включение УРК не произведено либо завершилось с ошибкой.", Index));
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Unit.Journal.Informarion(Unit, (int)(2002 + 10 * Index), String.Format("Телеуправление - ВА №{0}, включение УРК произведено.", Index));
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

            overlay.Visibility = System.Windows.Visibility.Visible;
            progress.IsActive = true;

            lockUpdate = true;
            Unit.Journal.Informarion(Unit, (int)(2003 + 10 * Index), String.Format("Телеуправление - ВА №{0}, отключение УРК...", Index));
            Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("rect{0}-ka-ctrl", Index), "B"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("B"))
                    {
                        Unit.Journal.Error(Unit, (int)(2003 + 10 * Index), String.Format("Телеуправление - ВА №{0}, отключение УРК не произведено либо завершилось с ошибкой.", Index));
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Unit.Journal.Informarion(Unit, (int)(2003 + 10 * Index), String.Format("Телеуправление - ВА №{0}, отключение УРК произведено.", Index));
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
            kaOffButton.IsEnabled = (kaOffCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);

            progress.IsActive = false;

            if (Unit.IsOnline)
            {
                alertTimer.Stop();
                overlay.Visibility = System.Windows.Visibility.Hidden;                
                alertMessageBlock.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                alertTimer.Start();
                overlay.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}

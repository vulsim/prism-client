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
        private Boolean lockUpdate = false;
        private System.Timers.Timer alertTimer;

        public Lsw9Control(Unit unit, String title)
        {
            InitializeComponent();

            this.Unit = unit;
            this.titleText.Text = title;

            alertTimer = new System.Timers.Timer(1000);
            alertTimer.Elapsed += AlertTimerEvent;

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

         ~Lsw9Control()
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

        private void UpdateState()
        {
            lsw9StateQfTile.State = Unit.Processing.Params["lsw9_state_qf_switch"].State;
            lsw9StateQsTile.State = Unit.Processing.Params["lsw9_state_qs_switch"].State;
            lsw9StateTcTile.State = Unit.Processing.Params["lsw9_state_tc_switch"].State;            
            lsw9AlarmCircuitTile.State = Unit.Processing.Params["lsw9_alarm_circuit_fault"].State;

            if (lockUpdate)
            {
                return;
            }

            onButton.IsEnabled = (qfOnCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);
            offButton.IsEnabled = (qfOffCtrlState.State == ParamState.A && Unit.Processing.IsAvaliable);

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

        private void OffButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            overlay.Visibility = System.Windows.Visibility.Visible;
            progress.IsActive = true;

            lockUpdate = true;
            Unit.Journal.Informarion(Unit, 3091, "Телеуправление - АЗ, отключение...");
            Unit.Processing.Operate(new ProducerChannelValue("auto", "lsw9-qf-ctrl", "B"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("B"))
                    {
                        Unit.Journal.Error(Unit, 3091, "Телеуправление - АЗ, отключение не произведено либо завершилось с ошибкой.");
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Unit.Journal.Informarion(Unit, 3091, "Телеуправление - АЗ, отключение произведено.");
                    }
                    lockUpdate = false;
                    UpdateState();
                });
            });
        }

        private void onButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            overlay.Visibility = System.Windows.Visibility.Visible;
            progress.IsActive = true;

            lockUpdate = true;
            Unit.Journal.Informarion(Unit, 3090, "Телеуправление - АЗ, включение...");
            Unit.Processing.Operate(new ProducerChannelValue("auto", "lsw9-qf-ctrl", "A"), delegate(string error, ProducerChannelValue value)
            {
                MainThread.EnqueueTask(delegate()
                {
                    if (error != null || value == null || !value.Value.Equals("A"))
                    {
                        Unit.Journal.Error(Unit, 3090, "Телеуправление - АЗ, включение не произведено либо завершилось с ошибкой.");
                        errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Unit.Journal.Informarion(Unit, 3090, "Телеуправление - АЗ, включение произведено.");
                    }
                    lockUpdate = false;
                    UpdateState();
                });
            });
        }
    }
}

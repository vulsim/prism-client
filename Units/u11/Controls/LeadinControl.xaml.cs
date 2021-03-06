﻿using System;
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
    /// Логика взаимодействия для LeadinControl.xaml
    /// </summary>
    public partial class LeadinControl : UserControl
    {
        private Unit Unit;
        private uint Index;

        private List<string> ParamRelations;
        private Param onCtrlState;
        private Param offCtrlState;        
        private Boolean lockUpdate = false;
        private System.Timers.Timer alertTimer;

        public LeadinControl(Unit unit, uint index, String title)
        {
            ParamRelations = new List<string>();
            InitializeComponent();

            this.Unit = unit;
            this.Index = index;
            this.titleText.Text = title;

            alertTimer = new System.Timers.Timer(1000);
            alertTimer.Elapsed += AlertTimerEvent;

            if (Index < 3)
            {
                ParamRelations.Add(String.Format("leadin{0}_state_in_switch", Index));
                ParamRelations.Add(String.Format("leadin{0}_state_tc_switch", Index));
                ParamRelations.Add(String.Format("leadin{0}_alarm_in_switch_fault", Index));
                ParamRelations.Add(String.Format("leadin{0}_alarm_circuit_fault", Index));
                
                onCtrlState = new Param("leadin_on_ctrl_state", new List<ParamRelation> 
                { 
                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params[String.Format("leadin{0}_state_in_switch", Index)], ParamState.A)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params[String.Format("leadin{0}_state_tc_switch", Index)], ParamState.B)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params[String.Format("leadin{0}_alarm_in_switch_fault", Index)], ParamState.C)
                    }, ParamState.Idle),

                     new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params[String.Format("leadin{0}_alarm_circuit_fault", Index)], ParamState.C)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                    
                    }, ParamState.A)
                });

                offCtrlState = new Param("leadin_off_ctrl_state", new List<ParamRelation> 
                { 
                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params[String.Format("leadin{0}_state_in_switch", Index)], ParamState.B)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params[String.Format("leadin{0}_state_tc_switch", Index)], ParamState.B)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params[String.Format("leadin{0}_alarm_in_switch_fault", Index)], ParamState.C)
                    }, ParamState.Idle),

                     new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params[String.Format("leadin{0}_alarm_circuit_fault", Index)], ParamState.C)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                    
                    }, ParamState.A)
                });
            }
            else
            {
                ParamRelations.Add("ol_state_in_switch");
                ParamRelations.Add("ol_state_tc_switch");
                ParamRelations.Add("ol_alarm_switch_fault");
                ParamRelations.Add("ol_alarm_circuit_fault");

                onCtrlState = new Param("ol_on_ctrl_state", new List<ParamRelation> 
                { 
                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params["ol_state_in_switch"], ParamState.A)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params["ol_state_tc_switch"], ParamState.B)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params["ol_alarm_switch_fault"], ParamState.C)
                    }, ParamState.Idle),

                     new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params["ol_alarm_circuit_fault"], ParamState.C)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                    
                    }, ParamState.A)
                });

                offCtrlState = new Param("ol_off_ctrl_state", new List<ParamRelation> 
                { 
                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params["ol_state_in_switch"], ParamState.B)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params["ol_state_tc_switch"], ParamState.B)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params["ol_alarm_switch_fault"], ParamState.C)
                    }, ParamState.Idle),

                     new ParamRelation(new List<ParamCombination> 
                    { 
                        new ParamCombination(Unit.Processing.Params["ol_alarm_circuit_fault"], ParamState.C)
                    }, ParamState.Idle),

                    new ParamRelation(new List<ParamCombination> 
                    { 
                    
                    }, ParamState.A)
                });
            }
        }

        ~LeadinControl()
        {
            alertTimer.Stop();
            alertTimer.Elapsed -= AlertTimerEvent;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(Index < 3))
            {
                leadinAlarmTsnLostTile.Visibility = System.Windows.Visibility.Hidden;
                leadinAlarmTnRu6Tile.Visibility = System.Windows.Visibility.Hidden;
                leadinAlarmTnCircuitTile.Visibility = System.Windows.Visibility.Hidden;
            }
            
            Unit.Processing.ProcessingParamUpdateEvent += delegate(object sender1, Param param)
            {
                if (ParamRelations.Contains(param.Name))
                {
                    MainThread.EnqueueTask(delegate()
                    {
                        UpdateState();
                    });
                }
            };

            Unit.Processing.ProcessingOnlineStateChangedEvent += delegate(object sender2, bool isOnline)
            {
                MainThread.EnqueueTask(delegate()
                {
                    UpdateState();
                });
            };

            UpdateState();
        }

        private void AlertTimerEvent(object sender, ElapsedEventArgs e)
        {
            MainThread.EnqueueTask(delegate()
            {
                alertMessageBlock.Visibility = (alertMessageBlock.Visibility == System.Windows.Visibility.Hidden) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            });
        }

        private void onButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;          
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            overlay.Visibility = System.Windows.Visibility.Visible;
            progress.IsActive = true;

            if (Index < 3)
            {
                lockUpdate = true;
                Unit.Journal.Informarion(Unit, (int)(1000 + Index * 10), (Index == 1) ? "Телеуправление - Рабочий ввод, включение..." : "Телеуправление - Резервный ввод, включение...");
                Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("leadin{0}-ctrl", Index), "A"), new TimeSpan(0, 0, 15), delegate(string error, ProducerChannelValue value)
                {
                    MainThread.EnqueueTask(delegate()
                    {
                        if (error != null || value == null || !value.Value.Equals("A"))
                        {
                            Unit.Journal.Error(Unit, (int)(1000 + Index * 10), (Index == 1) ? "Телеуправление - Рабочий ввод, включение не произведено либо завершилось с ошибкой." : "Телеуправление - Резервный ввод, включение не произведено либо завершилось с ошибкой.");
                            errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            Unit.Journal.Informarion(Unit, (int)(1000 + Index * 10), (Index == 1) ? "Телеуправление - Рабочий ввод, включение произведено." : "Телеуправление - Резервный ввод, включение произведено.");
                        }
                        lockUpdate = false;
                        UpdateState();
                    });
                });
            }
            else
            {
                lockUpdate = true;
                Unit.Journal.Informarion(Unit, (int)(1000 + Index * 10), "Телеуправление - Отходящая линия, включение...");
                Unit.Processing.Operate(new ProducerChannelValue("auto", "ol-ctrl", "A"), new TimeSpan(0, 0, 15), delegate(string error, ProducerChannelValue value)
                {
                    MainThread.EnqueueTask(delegate()
                    {
                        if (error != null || value == null || !value.Value.Equals("A"))
                        {
                            Unit.Journal.Error(Unit, (int)(1000 + Index * 10), "Телеуправление - Отходящая линия, включение не произведено либо завершилось с ошибкой.");
                            errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            Unit.Journal.Informarion(Unit, (int)(1000 + Index * 10), "Телеуправление - Отходящая линия, включение произведено.");
                        }
                        lockUpdate = false;
                        UpdateState();
                    });
                });
            }            
        }

        private void offButton_Click(object sender, RoutedEventArgs e)
        {
            onButton.IsEnabled = false;
            offButton.IsEnabled = false;
            errorMessagBlock.Visibility = System.Windows.Visibility.Hidden;

            overlay.Visibility = System.Windows.Visibility.Visible;
            progress.IsActive = true;

            if (Index < 3)
            {
                lockUpdate = true;
                Unit.Journal.Informarion(Unit, (int)(1001 + Index * 10), (Index == 1) ? "Телеуправление - Рабочий ввод, отключение..." : "Телеуправление - Резервный ввод, отключение...");
                Unit.Processing.Operate(new ProducerChannelValue("auto", String.Format("leadin{0}-ctrl", Index), "B"), new TimeSpan(0, 0, 15), delegate(string error, ProducerChannelValue value)
                {
                    MainThread.EnqueueTask(delegate()
                    {
                        if (error != null || value == null || !value.Value.Equals("B"))
                        {
                            Unit.Journal.Error(Unit, (int)(1001 + Index * 10), (Index == 1) ? "Телеуправление - Рабочий ввод, отключение не произведено либо завершилось с ошибкой." : "Телеуправление - Резервный ввод, отключение не произведено либо завершилось с ошибкой.");
                            errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            Unit.Journal.Informarion(Unit, (int)(1001 + Index * 10), (Index == 1) ? "Телеуправление - Рабочий ввод, отключение произведено." : "Телеуправление - Резервный ввод, отключение произведено.");
                        }
                        lockUpdate = false;
                        UpdateState();
                    });
                });
            }
            else
            {
                lockUpdate = true;
                Unit.Journal.Informarion(Unit, (int)(1001 + Index * 10), "Телеуправление - Отходящая линия, отключение...");
                Unit.Processing.Operate(new ProducerChannelValue("auto", "ol-ctrl", "B"), new TimeSpan(0, 0, 15), delegate(string error, ProducerChannelValue value)
                {
                    MainThread.EnqueueTask(delegate()
                    {
                        if (error != null || value == null || !value.Value.Equals("B"))
                        {
                            errorMessagBlock.Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            Unit.Journal.Error(Unit, (int)(1001 + Index * 10), "Телеуправление - Отходящая линия, отключение не произведено либо завершилось с ошибкой.");
                            Unit.Journal.Informarion(Unit, (int)(1001 + Index * 10), "Телеуправление - Отходящая линия, отключение произведено.");
                        }
                        lockUpdate = false;
                        UpdateState();
                    });
                });
            } 
        }

        private void UpdateState()
        {
            if (Index < 3)
            {
                leadinStateSwitchTile.State = Unit.Processing.Params[String.Format("leadin{0}_state_in_switch", Index)].State;
                leadinStateTcTile.State = Unit.Processing.Params[String.Format("leadin{0}_state_tc_switch", Index)].State;
                leadinAlarmTsnLostTile.State = Unit.Processing.Params[String.Format("leadin{0}_alarm_tsn_lost_power", Index)].State;
                leadinAlarmTnRu6Tile.State = Unit.Processing.Params[String.Format("leadin{0}_alarm_tn_ru6kv_fault", Index)].State;
                leadinAlarmTnCircuitTile.State = Unit.Processing.Params[String.Format("leadin{0}_alarm_tn_circuit_fault", Index)].State;
                leadinAlarmSwitchFaultTile.State = Unit.Processing.Params[String.Format("leadin{0}_alarm_in_switch_fault", Index)].State;
                leadinAlarmCircuitTile.State = Unit.Processing.Params[String.Format("leadin{0}_alarm_circuit_fault", Index)].State;
                energyTotalEplus.Text = Unit.Processing.Params[String.Format("leadin{0}_total_eplus_energy", Index)].Value;
                energyTotalEminus.Text = Unit.Processing.Params[String.Format("leadin{0}_total_eminus_energy", Index)].Value;
                energyMonthEplus.Text = Unit.Processing.Params[String.Format("leadin{0}_month_eplus_energy", Index)].Value;
                energyMonthEminus.Text = Unit.Processing.Params[String.Format("leadin{0}_month_eminus_energy", Index)].Value;
                energyTotalRplus.Text = Unit.Processing.Params[String.Format("leadin{0}_total_rplus_energy", Index)].Value;
                energyTotalRminus.Text = Unit.Processing.Params[String.Format("leadin{0}_total_rminus_energy", Index)].Value;
                energyMonthRplus.Text = Unit.Processing.Params[String.Format("leadin{0}_month_rplus_energy", Index)].Value;
                energyMonthRminus.Text = Unit.Processing.Params[String.Format("leadin{0}_month_rminus_energy", Index)].Value;
            }
            else
            {
                leadinStateSwitchTile.State = Unit.Processing.Params["ol_state_in_switch"].State;
                leadinStateTcTile.State = Unit.Processing.Params["ol_state_tc_switch"].State;
                leadinAlarmSwitchFaultTile.State = Unit.Processing.Params["ol_alarm_switch_fault"].State;
                leadinAlarmCircuitTile.State = Unit.Processing.Params["ol_alarm_circuit_fault"].State;
                energyTotalEplus.Text = Unit.Processing.Params["ol_total_eplus_energy"].Value;
                energyTotalEminus.Text = Unit.Processing.Params["ol_total_eminus_energy"].Value;
                energyMonthEplus.Text = Unit.Processing.Params["ol_month_eplus_energy"].Value;
                energyMonthEminus.Text = Unit.Processing.Params["ol_month_eminus_energy"].Value;
                energyTotalRplus.Text = Unit.Processing.Params["ol_total_rplus_energy"].Value;
                energyTotalRminus.Text = Unit.Processing.Params["ol_total_rminus_energy"].Value;
                energyMonthRplus.Text = Unit.Processing.Params["ol_month_rplus_energy"].Value;
                energyMonthRminus.Text = Unit.Processing.Params["ol_month_rminus_energy"].Value;
            }

            if (lockUpdate)
            {
                return;
            }

            onButton.IsEnabled = (onCtrlState.State == ParamState.A && Unit.Processing.IsOnline);
            offButton.IsEnabled = (offCtrlState.State == ParamState.A && Unit.Processing.IsOnline);

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

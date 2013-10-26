using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Prism.General;
using Prism.General.Automation;

namespace Prism.Units.Classes
{
    public delegate void ProcessingUpdateEventHandler(object sender);
    public delegate void ProcessingChangeStateEventHandler(object sender);
    public delegate void ProcessingOperateCallback(string error, ProducerChannelValue value);

    public class Processing
    {
        private Producer producer;
        ProducerSettings producerSettings;
        private System.Timers.Timer autoUpdateTimer;
        private bool IsAutoUpdateInProgress;
        private bool IsQueryAvaliable;
        private uint OperateRetainCount;

        public Dictionary<string, string> ChannelValues;
        public Dictionary<string, Param> Params;

        public event ProcessingUpdateEventHandler ProcessingUpdateEvent;
        public event ProcessingChangeStateEventHandler ProcessingChangeStateEvent;

        public bool IsAvaliable { get { return IsQueryAvaliable; } }
        public bool IsBusy { get { return IsAutoUpdateInProgress || (OperateRetainCount > 0); } }

        public Processing(UnitSettings settings)
        {
            ChannelValues = new Dictionary<string, string>();
            Params = new Dictionary<string, Param>();
            
            Params["leadin1_state_in_switch"] = new Param("leadin1_state_in_switch", ChannelValues, "io,di-rab-908", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1")}));
            Params["leadin1_state_tc_switch"] = new Param("leadin1_state_tc_switch", ChannelValues, "io,di-rab-916", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));            
            Params["leadin1_alarm_in_switch_fault"] = new Param("leadin1_alarm_in_switch_fault", ChannelValues, "io,di-rab-910", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["leadin1_alarm_circuit_fault"] = new Param("leadin1_alarm_circuit_fault", ChannelValues, "io,di-rab-912", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["leadin1_alarm_tn_circuit_fault"] = new Param("leadin1_alarm_tn_circuit_fault", ChannelValues, "io,di-tn1-918", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["leadin1_alarm_tn_ru6kv_fault"] = new Param("leadin1_alarm_tn_ru6kv_fault", ChannelValues, "io,di-tn1-920", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["leadin1_alarm_tsn_lost_power"] = new Param("leadin1_alarm_tsn_lost_power", ChannelValues, "io,di-tsn1-ts71", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            
            Params["leadin1_state"] = new Param("leadin1_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state_in_switch"], ParamState.A), 
                    new ParamCombination(Params["leadin1_alarm_in_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tn_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tn_ru6kv_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tsn_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state_in_switch"], ParamState.B), 
                    new ParamCombination(Params["leadin1_alarm_in_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tn_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tn_ru6kv_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin1_alarm_tsn_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["leadin2_state_in_switch"] = new Param("leadin2_state_in_switch", ChannelValues, "io,di-rez-900", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1")}));
            Params["leadin2_state_tc_switch"] = new Param("leadin2_state_tc_switch", ChannelValues, "io,di-rez-906", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));            
            Params["leadin2_alarm_in_switch_fault"] = new Param("leadin2_alarm_in_switch_fault", ChannelValues, "io,di-rez-902", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["leadin2_alarm_circuit_fault"] = new Param("leadin1_alarm_circuit_fault", ChannelValues, "io,di-rez-904", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["leadin2_alarm_tn_circuit_fault"] = new Param("leadin1_alarm_tn_circuit_fault", ChannelValues, "io,di-tn2-918", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["leadin2_alarm_tn_ru6kv_fault"] = new Param("leadin1_alarm_tn_ru6kv_fault", ChannelValues, "io,di-tn2-741", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["leadin2_alarm_tsn_lost_power"] = new Param("leadin1_alarm_tsn_lost_power", ChannelValues, "io,di-tsn2-ts71", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["leadin2_state"] = new Param("leadin2_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin2_state_in_switch"], ParamState.A), 
                    new ParamCombination(Params["leadin2_alarm_in_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tn_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tn_ru6kv_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tsn_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin2_state_in_switch"], ParamState.B), 
                    new ParamCombination(Params["leadin2_alarm_in_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tn_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tn_ru6kv_fault"], ParamState.Idle),
                    new ParamCombination(Params["leadin2_alarm_tsn_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });
            
            Params["rect1_state_pa_switch"] = new Param("rect1_state_pa_switch", ChannelValues, "io,di-pa1-911", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1")}));
            Params["rect1_state_qs_switch"] = new Param("rect1_state_qs_switch", ChannelValues, "io,di-ru1-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["rect1_state_qf_switch"] = new Param("rect1_state_qf_switch", ChannelValues, "io,di-ru1-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["rect1_state_tc_switch"] = new Param("rect1_state_tc_switch", ChannelValues, "io,di-ru1-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["rect1_alarm_circuit_fault"] = new Param("rect1_alarm_circuit_fault", ChannelValues, "io,di-ka1-n02", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect1_alarm_pa_switch_fault"] = new Param("rect1_alarm_pa_switch_fault", ChannelValues, "io,di-pa1-1003", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect1_alarm_rec_fault"] = new Param("rect1_alarm_rec_fault", ChannelValues, "io,di-v1-67", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect1_alarm_rec_gas_warn"] = new Param("rect1_alarm_rec_gas_warn", ChannelValues, "io,di-v1-86", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect1_alarm_rec_overload"] = new Param("rect1_alarm_rec_overload", ChannelValues, "io,di-v1-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect1_alarm_rec_rpz600v_fault"] = new Param("rect1_alarm_rec_rpz600v_fault", ChannelValues, "io,di-v1-106", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["rect1_state"] = new Param("rect1_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state_pa_switch"], ParamState.A),
                    new ParamCombination(Params["rect1_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["rect1_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["rect1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_rpz600v_fault"], ParamState.Idle)             
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect1_alarm_rec_rpz600v_fault"], ParamState.Idle)                
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });           

            Params["rect2_state_pa_switch"] = new Param("rect2_state_pa_switch", ChannelValues, "io,di-pa2-911", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1")}));
            Params["rect2_state_qs_switch"] = new Param("rect2_state_qs_switch", ChannelValues, "io,di-ru2-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["rect2_state_qf_switch"] = new Param("rect2_state_qf_switch", ChannelValues, "io,di-ru2-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["rect2_state_tc_switch"] = new Param("rect2_state_tc_switch", ChannelValues, "io,di-ru2-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["rect2_alarm_circuit_fault"] = new Param("rect2_alarm_circuit_fault", ChannelValues, "io,di-ka2-n02", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect2_alarm_pa_switch_fault"] = new Param("rect2_alarm_pa_switch_fault", ChannelValues, "io,di-pa2-1003", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect2_alarm_rec_fault"] = new Param("rect2_alarm_rec_fault", ChannelValues, "io,di-v2-67", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect2_alarm_rec_gas_warn"] = new Param("rect2_alarm_rec_gas_warn", ChannelValues, "io,di-v2-86", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect2_alarm_rec_overload"] = new Param("rect2_alarm_rec_overload", ChannelValues, "io,di-v2-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect2_alarm_rec_rpz600v_fault"] = new Param("rect2_alarm_rec_rpz600v_fault", ChannelValues, "io,di-v2-106", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["rect2_state"] = new Param("rect2_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect2_state_pa_switch"], ParamState.A),
                    new ParamCombination(Params["rect2_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["rect2_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["rect2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_rpz600v_fault"], ParamState.Idle)             
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect2_alarm_rec_rpz600v_fault"], ParamState.Idle)                
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            }); 

            Params["rect3_state_pa_switch"] = new Param("rect3_state_pa_switch", ChannelValues, "io,di-pa3-911", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1")}));
            Params["rect3_state_qs_switch"] = new Param("rect3_state_qs_switch", ChannelValues, "io,di-ru3-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["rect3_state_qf_switch"] = new Param("rect3_state_qf_switch", ChannelValues, "io,di-ru3-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["rect3_state_tc_switch"] = new Param("rect3_state_tc_switch", ChannelValues, "io,di-ru3-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["rect3_alarm_circuit_fault"] = new Param("rect3_alarm_circuit_fault", ChannelValues, "io,di-ka3-n02", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect3_alarm_pa_switch_fault"] = new Param("rect3_alarm_pa_switch_fault", ChannelValues, "io,di-pa3-1003", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect3_alarm_rec_fault"] = new Param("rect3_alarm_rec_fault", ChannelValues, "io,di-v3-67", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect3_alarm_rec_gas_warn"] = new Param("rect3_alarm_rec_gas_warn", ChannelValues, "io,di-v3-86", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect3_alarm_rec_overload"] = new Param("rect3_alarm_rec_overload", ChannelValues, "io,di-v3-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["rect3_alarm_rec_rpz600v_fault"] = new Param("rect3_alarm_rec_rpz600v_fault", ChannelValues, "io,di-v3-106", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["rect3_state"] = new Param("rect3_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect3_state_pa_switch"], ParamState.A),
                    new ParamCombination(Params["rect3_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["rect3_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["rect3_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_rpz600v_fault"], ParamState.Idle)             
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect3_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_pa_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_fault"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_gas_warn"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_overload"], ParamState.Idle),
                    new ParamCombination(Params["rect3_alarm_rec_rpz600v_fault"], ParamState.Idle)                
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            }); 

            Params["lsw1_state_qs_switch"] = new Param("lsw1_state_qs_switch", ChannelValues, "io,di-ul1-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw1_state_qf_switch"] = new Param("lsw1_state_qf_switch", ChannelValues, "io,di-ul1-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw1_state_tc_switch"] = new Param("lsw1_state_tc_switch", ChannelValues, "io,di-ul1-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw1_state_spare_switch"] = new Param("lsw1_state_spare_switch", ChannelValues, "io,di-ul1-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw1_alarm_short_fault"] = new Param("lsw1_alarm_short_fault", ChannelValues, "io,di-ul1-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw1_alarm_circuit_fault"] = new Param("lsw1_alarm_circuit_fault", ChannelValues, "io,di-ul1-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw1_alarm_600v_lost_power"] = new Param("lsw1_alarm_600v_lost_power", ChannelValues, "io,di-ul1-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["lsw1_state"] = new Param("lsw1_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw1_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw1_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw1_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw1_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw1_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });
       
            Params["lsw2_state_qs_switch"] = new Param("lsw2_state_qs_switch", ChannelValues, "io,di-ul2-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw2_state_qf_switch"] = new Param("lsw2_state_qf_switch", ChannelValues, "io,di-ul2-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw2_state_tc_switch"] = new Param("lsw2_state_tc_switch", ChannelValues, "io,di-ul2-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw2_state_spare_switch"] = new Param("lsw2_state_spare_switch", ChannelValues, "io,di-ul2-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw2_alarm_short_fault"] = new Param("lsw2_alarm_short_fault", ChannelValues, "io,di-ul2-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw2_alarm_circuit_fault"] = new Param("lsw2_alarm_circuit_fault", ChannelValues, "io,di-ul2-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw2_alarm_600v_lost_power"] = new Param("lsw2_alarm_600v_lost_power", ChannelValues, "io,di-ul2-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["lsw2_state"] = new Param("lsw2_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw2_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw2_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw2_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw2_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw2_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw2_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw2_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw3_state_qs_switch"] = new Param("lsw3_state_qs_switch", ChannelValues, "io,di-ul3-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw3_state_qf_switch"] = new Param("lsw3_state_qf_switch", ChannelValues, "io,di-ul3-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw3_state_tc_switch"] = new Param("lsw3_state_tc_switch", ChannelValues, "io,di-ul3-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw3_state_spare_switch"] = new Param("lsw3_state_spare_switch", ChannelValues, "io,di-ul3-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw3_alarm_short_fault"] = new Param("lsw3_alarm_short_fault", ChannelValues, "io,di-ul3-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw3_alarm_circuit_fault"] = new Param("lsw3_alarm_circuit_fault", ChannelValues, "io,di-ul3-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw3_alarm_600v_lost_power"] = new Param("lsw3_alarm_600v_lost_power", ChannelValues, "io,di-ul3-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["lsw3_state"] = new Param("lsw3_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw3_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw3_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw3_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw3_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw3_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw3_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw3_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw3_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw4_state_qs_switch"] = new Param("lsw4_state_qs_switch", ChannelValues, "io,di-ul4-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw4_state_qf_switch"] = new Param("lsw4_state_qf_switch", ChannelValues, "io,di-ul4-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw4_state_tc_switch"] = new Param("lsw4_state_tc_switch", ChannelValues, "io,di-ul4-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw4_state_spare_switch"] = new Param("lsw4_state_spare_switch", ChannelValues, "io,di-ul4-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw4_alarm_short_fault"] = new Param("lsw4_alarm_short_fault", ChannelValues, "io,di-ul4-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw4_alarm_circuit_fault"] = new Param("lsw4_alarm_circuit_fault", ChannelValues, "io,di-ul4-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw4_alarm_600v_lost_power"] = new Param("lsw4_alarm_600v_lost_power", ChannelValues, "io,di-ul4-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["lsw4_state"] = new Param("lsw4_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw4_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw4_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw4_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw4_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw4_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw4_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw4_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw4_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw5_state_qs_switch"] = new Param("lsw5_state_qs_switch", ChannelValues, "io,di-ul5-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw5_state_qf_switch"] = new Param("lsw5_state_qf_switch", ChannelValues, "io,di-ul5-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw5_state_tc_switch"] = new Param("lsw5_state_tc_switch", ChannelValues, "io,di-ul5-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw5_state_spare_switch"] = new Param("lsw5_state_spare_switch", ChannelValues, "io,di-ul5-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw5_alarm_short_fault"] = new Param("lsw5_alarm_short_fault", ChannelValues, "io,di-ul5-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw5_alarm_circuit_fault"] = new Param("lsw5_alarm_circuit_fault", ChannelValues, "io,di-ul5-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));
            Params["lsw5_alarm_600v_lost_power"] = new Param("lsw5_alarm_600v_lost_power", ChannelValues, "io,di-ul5-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["lsw5_state"] = new Param("lsw5_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw5_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw5_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw5_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw5_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw5_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw5_alarm_short_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw5_alarm_circuit_fault"], ParamState.Idle),
                    new ParamCombination(Params["lsw5_alarm_600v_lost_power"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["lsw9_state_qs_switch"] = new Param("lsw9_state_qs_switch", ChannelValues, "io,di-zap-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw9_state_qf_switch"] = new Param("lsw9_state_qf_switch", ChannelValues, "io,di-zap-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw9_state_tc_switch"] = new Param("lsw9_state_tc_switch", ChannelValues, "io,di-zap-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1")}));
            Params["lsw9_alarm_circuit_fault"] = new Param("lsw9_alarm_circuit_fault", ChannelValues, "io,di-zap-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1")}));

            Params["lsw9_state"] = new Param("lsw9_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw9_state_qs_switch"], ParamState.A),
                    new ParamCombination(Params["lsw9_state_qf_switch"], ParamState.A),
                    new ParamCombination(Params["lsw9_alarm_circuit_fault"], ParamState.Idle),
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw9_alarm_circuit_fault"], ParamState.Idle),
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["general_state_ol_switch"] = new Param("general_state_ol_switch", ChannelValues, "io,di-ol-908", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_state_ol_tc_switch"] = new Param("general_state_ol_tc_switch", ChannelValues, "io,di-ol-916", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_alarm_ol_switch_fault"] = new Param("general_alarm_ol_switch_fault", ChannelValues, "io,di-ol-910", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["general_alarm_ol_circuit_fault"] = new Param("general_alarm_ol_circuit_fault", ChannelValues, "io,di-ol-912", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["general_state_sn_automation"] = new Param("general_state_sn_automation", ChannelValues, "io,di-sn-238", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_state_sn_leadin1"] = new Param("general_state_sn_leadin1", ChannelValues, "io,di-sn-242", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_state_sn_leadin2"] = new Param("general_state_sn_leadin2", ChannelValues, "io,di-sn-244", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["general_alarm_sn_24v_lost_power"] = new Param("general_alarm_sn_24v_lost_power", ChannelValues, "io,di-sn-339", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["general_alarm_fire_alarm"] = new Param("general_alarm_fire_alarm", ChannelValues, "io,di-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["general_alarm_intrusion_alarm"] = new Param("general_alarm_intrusion_alarm", ChannelValues, "io,di-113", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.B, "1") }));

            Params["common_group1_state"] = new Param("common_group1_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state"], ParamState.A),
                    new ParamCombination(Params["leadin2_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["leadin1_state"], ParamState.B),
                    new ParamCombination(Params["leadin2_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["common_group2_state"] = new Param("common_group2_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.A),
                    new ParamCombination(Params["rect2_state"], ParamState.A),
                    new ParamCombination(Params["rect3_state"], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.B),
                    new ParamCombination(Params["rect2_state"], ParamState.A),
                    new ParamCombination(Params["rect3_state"], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.A),
                    new ParamCombination(Params["rect2_state"], ParamState.B),
                    new ParamCombination(Params["rect3_state"], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.A),
                    new ParamCombination(Params["rect2_state"], ParamState.A),
                    new ParamCombination(Params["rect3_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.A),
                    new ParamCombination(Params["rect2_state"], ParamState.B),
                    new ParamCombination(Params["rect3_state"], ParamState.B)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.B),
                    new ParamCombination(Params["rect2_state"], ParamState.A),
                    new ParamCombination(Params["rect3_state"], ParamState.B)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["rect1_state"], ParamState.B),
                    new ParamCombination(Params["rect2_state"], ParamState.B),
                    new ParamCombination(Params["rect3_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["common_group3_state"] = new Param("common_group3_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    new ParamCombination(Params["lsw5_state"], ParamState.A),
                    new ParamCombination(Params["lsw9_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.B),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    new ParamCombination(Params["lsw5_state"], ParamState.A),
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.B),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    new ParamCombination(Params["lsw5_state"], ParamState.A),
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.B),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    new ParamCombination(Params["lsw5_state"], ParamState.A),
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.B),
                    new ParamCombination(Params["lsw5_state"], ParamState.A),
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    new ParamCombination(Params["lsw5_state"], ParamState.B),
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });
            
            Params["common_group4_state"] = new Param("common_group4_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["general_state_sn_automation"], ParamState.A),
                    new ParamCombination(Params["general_alarm_sn_24v_lost_power"], ParamState.Idle),
                    new ParamCombination(Params["general_alarm_fire_alarm"], ParamState.Idle),
                    new ParamCombination(Params["general_alarm_intrusion_alarm"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["general_alarm_sn_24v_lost_power"], ParamState.Idle),
                    new ParamCombination(Params["general_alarm_fire_alarm"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["common_state"] = new Param("common_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["common_group1_state"], ParamState.A),
                    new ParamCombination(Params["common_group1_state"], ParamState.A),
                    new ParamCombination(Params["common_group1_state"], ParamState.A),
                    new ParamCombination(Params["common_group1_state"], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group1_state"], ParamState.B)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group2_state"], ParamState.B)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group3_state"], ParamState.B)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group4_state"], ParamState.B)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            producerSettings = new ProducerSettings();
            producerSettings.ReqAddr = settings.Connection.ReqAddr;
            producerSettings.SubAddr = settings.Connection.SubAddr;

            producerSettings.Channels.Add(new ProducerChannel("io", "di-rab-910"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-rab-908"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-rab-916"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-rab-912"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-tsn1-ts71"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-tn1-918"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-tn1-920"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-rez-902"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-rez-900"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-rez-906"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-rez-904"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-tsn2-ts71"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-tn2-918"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-tn2-741"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v1-86"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v1-111"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v1-106"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v1-67"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ru1-708"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ru1-710"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ru1-712"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ka1-n02"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-pa1-1003"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-pa1-911"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v2-86"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v2-111"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v2-106"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v2-67"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ru2-708"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ru2-710"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ru2-712"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ka2-n02"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-pa2-1003"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-pa2-911"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v3-86"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v3-111"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v3-106"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-v3-67"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ru3-708"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ru3-710"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ru3-712"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ka3-n02"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-pa3-1003"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-pa3-911"));            
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul1-708"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul1-710"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul1-712"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul1-714"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul1-716"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul1-719"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul1-n01"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul2-708"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul2-710"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul2-712"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul2-714"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul2-716"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul2-719"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul2-n01"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul3-708"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul3-710"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul3-712"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul3-714"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul3-716"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul3-719"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul3-n01"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul4-708"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul4-710"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul4-712"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul4-714"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul4-716"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul4-719"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul4-n01"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul5-708"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul5-710"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul5-712"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul5-714"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul5-716"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul5-719"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ul5-n01"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-zap-708"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-zap-710"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-zap-712"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-zap-n01"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-sn-238"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-sn-242"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-sn-244"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-sn-339"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-111"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-113"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ol-910"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ol-908"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ol-916"));
            producerSettings.Channels.Add(new ProducerChannel("io", "di-ol-912"));

            producer = new Producer(producerSettings);
            producer.Start();

            autoUpdateTimer = new System.Timers.Timer(60000);
            autoUpdateTimer.Elapsed += ProducerAutoUpdateEvent;            

            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                ProducerAutoUpdateEvent(null, null);
                producer.ChannelValueEvent += ProducerChannelValueEvent;
                autoUpdateTimer.Start();
            }, null);
        }

        ~Processing()
        {
            autoUpdateTimer.Stop();
            producer.ChannelValueEvent -= ProducerChannelValueEvent;
        }

        public void Operate(ProducerChannelValue value, ProcessingOperateCallback cb)
        {
            OperateRetainCount++;
            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                if (ProcessingChangeStateEvent != null)
                {
                    ProcessingChangeStateEvent(this);
                }
            }, null);

            producer.WriteChannelValue(value, delegate(string error, ProducerChannelValue result)
            {
                OperateRetainCount--;
                ThreadPool.QueueUserWorkItem(delegate(object target)
                {
                    if (ProcessingChangeStateEvent != null)
                    {
                        ProcessingChangeStateEvent(this);
                    }
                }, null);

                cb(error, result);
            });
        }

        private void ProducerAutoUpdateEvent(object sender, ElapsedEventArgs e)
        {
            if (IsAutoUpdateInProgress)
            {
                return;
            }

            IsAutoUpdateInProgress = true;
            int channelCounter = producerSettings.Channels.Count;
            ManualResetEvent continueEvent = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                if (ProcessingChangeStateEvent != null)
                {
                    ProcessingChangeStateEvent(this);
                }
            }, null);

            System.Timers.Timer queryUnvaliableTimer = new System.Timers.Timer(60000);
            queryUnvaliableTimer.Elapsed += delegate(object s, ElapsedEventArgs ev)
            {
                if (IsQueryAvaliable)
                {
                    IsQueryAvaliable = false;

                    ThreadPool.QueueUserWorkItem(delegate(object target)
                    {
                        if (ProcessingChangeStateEvent != null)
                        {
                            ProcessingChangeStateEvent(this);
                        }
                    }, null);
                }
                queryUnvaliableTimer.Stop();
                queryUnvaliableTimer = null;
            };
            queryUnvaliableTimer.Start();

            foreach (var channel in producerSettings.Channels)
            {
                producer.ReadChannelValue(channel, delegate(string error, ProducerChannelValue value)
                {
                    ChannelValues[value.Group + "," + value.Channel] = value.Value;                    
                    channelCounter--;

                    if (channelCounter <= 0)
                    {
                        continueEvent.Set();
                    }
                });
            }
            continueEvent.WaitOne();

            if (queryUnvaliableTimer != null)
            {
                queryUnvaliableTimer.Stop();
            }
            IsQueryAvaliable = true;
            IsAutoUpdateInProgress = false;

            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                if (ProcessingChangeStateEvent != null)
                {
                    ProcessingChangeStateEvent(this);
                }

                if (ProcessingUpdateEvent != null)
                {
                    ProcessingUpdateEvent(this);
                }
            }, null);
        }

        private void ProducerChannelValueEvent(object sender, ProducerChannelValue value)
        {
            ChannelValues[value.Group + "," + value.Channel] = value.Value;

            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                if (ProcessingUpdateEvent != null)
                {
                    ProcessingUpdateEvent(this);
                }
            }, null);
        }

        /*private void UpdateParams()
        {
            Params["leadin1_on"] = ("1").Equals(ChannelValues["io,di-rab-908"]) ? "3" : "4";
            Params["leadin1_tc_on"] = ("1").Equals(ChannelValues["io,di-rab-916"]) ? "2" : "3";
            Params["leadin1_tsn_lost_power"] = ("1").Equals(ChannelValues["io,di-tsn1-ts71"]) ? "5" : "2";
            Params["leadin1_fault"] = ("1").Equals(ChannelValues["io,di-rab-910"]) ? "5" : "2";
            Params["leadin1_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-rab-912"]) ? "5" : "2";
            Params["leadin1_tn_circuit_fault"] = ("1").Equals(ChannelValues["io,di-tn1-918"]) ? "5" : "2";
            Params["leadin1_tn_6kv_fault"] = ("1").Equals(ChannelValues["io,di-tn1-920"]) ? "5" : "2";

            Params["leadin2_on"] = ("1").Equals(ChannelValues["io,di-rez-900"]) ? "3" : "4";
            Params["leadin2_tc_on"] = ("1").Equals(ChannelValues["io,di-rez-906"]) ? "2" : "3";
            Params["leadin2_tsn_lost_power"] = ("1").Equals(ChannelValues["io,di-tsn2-ts71"]) ? "5" : "2";
            Params["leadin2_fault"] = ("1").Equals(ChannelValues["io,di-rez-902"]) ? "5" : "2";
            Params["leadin2_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-rez-904"]) ? "5" : "2";
            Params["leadin2_tn_circuit_fault"] = ("1").Equals(ChannelValues["io,di-tn2-918"]) ? "5" : "2";
            Params["leadin2_tn_6kv_fault"] = ("1").Equals(ChannelValues["io,di-tn2-741"]) ? "5" : "2";

            if (("3").Equals(Params["leadin1_on"]))
            {
                if (("5").Equals(Params["leadin1_tsn_lost_power"]) || ("5").Equals(Params["leadin1_fault"]) || ("5").Equals(Params["leadin1_control_circuit_fault"]) ||
                    ("5").Equals(Params["leadin1_tn_circuit_fault"]) || ("5").Equals(Params["leadin1_tn_6kv_fault"]))
                {
                    Params["leadin1_state"] = "9";
                }
                else
                {
                    Params["leadin1_state"] = "7";
                }
            }
            else
            {
                if (("5").Equals(Params["leadin1_tsn_lost_power"]) || ("5").Equals(Params["leadin1_fault"]) || ("5").Equals(Params["leadin1_control_circuit_fault"]) ||
                    ("5").Equals(Params["leadin1_tn_circuit_fault"]) || ("5").Equals(Params["leadin1_tn_6kv_fault"]))
                {
                    Params["leadin1_state"] = "5";
                }
                else if (("3").Equals(Params["leadin2_on"]))
                {
                    Params["leadin1_state"] = "4";
                }
                else
                {
                    Params["leadin1_state"] = "5";
                }
            }

            if (("3").Equals(Params["leadin2_on"]))
            {
                if (("5").Equals(Params["leadin2_tsn_lost_power"]) || ("5").Equals(Params["leadin2_fault"]) || ("5").Equals(Params["leadin2_control_circuit_fault"]) ||
                    ("5").Equals(Params["leadin2_tn_circuit_fault"]) || ("5").Equals(Params["leadin2_tn_6kv_fault"]))
                {
                    Params["leadin2_state"] = "9";
                }
                else
                {
                    Params["leadin2_state"] = "7";
                }
            }
            else
            {
                if (("5").Equals(Params["leadin2_tsn_lost_power"]) || ("5").Equals(Params["leadin2_fault"]) || ("5").Equals(Params["leadin2_control_circuit_fault"]) ||
                    ("5").Equals(Params["leadin2_tn_circuit_fault"]) || ("5").Equals(Params["leadin2_tn_6kv_fault"]))
                {
                    Params["leadin2_state"] = "5";
                }
                else if (("3").Equals(Params["leadin1_on"]))
                {
                    Params["leadin2_state"] = "4";
                }
                else
                {
                    Params["leadin2_state"] = "5";
                }
            }

            Params["rect1_pa_on"] = ("1").Equals(ChannelValues["io,di-pa1-911"]) ? "3" : "2";
            Params["rect1_tc_on"] = ("1").Equals(ChannelValues["io,di-ru1-708"]) ? "3" : "2";
            Params["rect1_qs_off"] = ("1").Equals(ChannelValues["io,di-ru1-710"]) ? "4" : "2";
            Params["rect1_qf_off"] = ("1").Equals(ChannelValues["io,di-ru1-712"]) ? "4" : "2";
            Params["rect1_v_overload"] = ("1").Equals(ChannelValues["io,di-v1-111"]) ? "5" : "2";
            Params["rect1_v_alarm"] = ("1").Equals(ChannelValues["io,di-v1-86"]) ? "5" : "2";
            Params["rect1_v_rpz600"] = ("1").Equals(ChannelValues["io,di-v1-106"]) ? "5" : "2";
            Params["rect1_v_fault"] = ("1").Equals(ChannelValues["io,di-v1-67"]) ? "5" : "2";
            Params["rect1_pa_fault"] = ("1").Equals(ChannelValues["io,di-pa1-1003"]) ? "5" : "2";
            Params["rect1_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ka1-n02"]) ? "5" : "2";
            Params["rect2_pa_on"] = ("1").Equals(ChannelValues["io,di-pa2-911"]) ? "3" : "2";
            Params["rect2_tc_on"] = ("1").Equals(ChannelValues["io,di-ru2-708"]) ? "3" : "2";
            Params["rect2_qs_off"] = ("1").Equals(ChannelValues["io,di-ru2-710"]) ? "4" : "2";
            Params["rect2_qf_off"] = ("1").Equals(ChannelValues["io,di-ru2-712"]) ? "4" : "2";
            Params["rect2_v_overload"] = ("1").Equals(ChannelValues["io,di-v2-111"]) ? "5" : "2";
            Params["rect2_v_alarm"] = ("1").Equals(ChannelValues["io,di-v2-86"]) ? "5" : "2";
            Params["rect2_v_rpz600"] = ("1").Equals(ChannelValues["io,di-v2-106"]) ? "5" : "2";
            Params["rect2_v_fault"] = ("1").Equals(ChannelValues["io,di-v2-67"]) ? "5" : "2";
            Params["rect2_pa_fault"] = ("1").Equals(ChannelValues["io,di-pa2-1003"]) ? "5" : "2";
            Params["rect2_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ka2-n02"]) ? "5" : "2";
            Params["rect3_pa_on"] = ("1").Equals(ChannelValues["io,di-pa3-911"]) ? "3" : "2";
            Params["rect3_tc_on"] = ("1").Equals(ChannelValues["io,di-ru3-708"]) ? "3" : "2";
            Params["rect3_qs_off"] = ("1").Equals(ChannelValues["io,di-ru3-710"]) ? "4" : "2";
            Params["rect3_qf_off"] = ("1").Equals(ChannelValues["io,di-ru3-712"]) ? "4" : "2";
            Params["rect3_v_overload"] = ("1").Equals(ChannelValues["io,di-v3-111"]) ? "5" : "2";
            Params["rect3_v_alarm"] = ("1").Equals(ChannelValues["io,di-v3-86"]) ? "5" : "2";
            Params["rect3_v_rpz600"] = ("1").Equals(ChannelValues["io,di-v3-106"]) ? "5" : "2";
            Params["rect3_v_fault"] = ("1").Equals(ChannelValues["io,di-v3-67"]) ? "5" : "2";
            Params["rect3_pa_fault"] = ("1").Equals(ChannelValues["io,di-pa3-1003"]) ? "5" : "2";
            Params["rect3_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ka3-n02"]) ? "5" : "2";

            if (("5").Equals(Params["rect1_v_overload"]) || ("5").Equals(Params["rect1_v_alarm"]) || ("5").Equals(Params["rect1_v_rpz600"]) ||
                ("5").Equals(Params["rect1_v_fault"]) || ("5").Equals(Params["rect1_pa_fault"]) || ("5").Equals(Params["rect1_control_circuit_fault"]))
            {
                Params["rect1_state"] = "5";
            }
            else if (("3").Equals(Params["rect1_pa_on"]) && ("2").Equals(Params["rect1_qs_off"]) && ("2").Equals(Params["rect1_qf_off"]))
            {
                Params["rect1_state"] = "3";
            }
            else
            {
                Params["rect1_state"] = "4";
            }

            if (("5").Equals(Params["rect2_v_overload"]) || ("5").Equals(Params["rect2_v_alarm"]) || ("5").Equals(Params["rect2_v_rpz600"]) ||
                ("5").Equals(Params["rect2_v_fault"]) || ("5").Equals(Params["rect2_pa_fault"]) || ("5").Equals(Params["rect2_control_circuit_fault"]))
            {
                Params["rect2_state"] = "5";
            }
            else if (("3").Equals(Params["rect2_pa_on"]) && ("2").Equals(Params["rect2_qs_off"]) && ("2").Equals(Params["rect2_qf_off"]))
            {
                Params["rect2_state"] = "3";
            }
            else
            {
                Params["rect2_state"] = "4";
            }

            if (("5").Equals(Params["rect3_v_overload"]) || ("5").Equals(Params["rect3_v_alarm"]) || ("5").Equals(Params["rect3_v_rpz600"]) ||
                ("5").Equals(Params["rect3_v_fault"]) || ("5").Equals(Params["rect3_pa_fault"]) || ("5").Equals(Params["rect3_control_circuit_fault"]))
            {
                Params["rect3_state"] = "5";
            }
            else if (("3").Equals(Params["rect3_pa_on"]) && ("2").Equals(Params["rect3_qs_off"]) && ("2").Equals(Params["rect3_qf_off"]))
            {
                Params["rect3_state"] = "3";
            }
            else
            {
                Params["rect3_state"] = "4";
            }
            
            Params["lsw1_tc_on"] = ("1").Equals(ChannelValues["io,di-ul1-708"]) ? "2" : "3";
            Params["lsw1_spare_switch_on"] = ("1").Equals(ChannelValues["io,di-ul1-719"]) ? "2" : "3";
            Params["lsw1_qs_off"] = ("1").Equals(ChannelValues["io,di-ul1-710"]) ? "4" : "3";
            Params["lsw1_qf_off"] = ("1").Equals(ChannelValues["io,di-ul1-712"]) ? "4" : "3";
            Params["lsw1_600v_fault"] = ("1").Equals(ChannelValues["io,di-ul1-714"]) ? "5" : "3";
            Params["lsw1_short_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul1-716"]) ? "5" : "2";            
            Params["lsw1_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul1-n01"]) ? "5" : "2";
            Params["lsw2_tc_on"] = ("1").Equals(ChannelValues["io,di-ul2-708"]) ? "2" : "3";
            Params["lsw2_spare_switch_on"] = ("1").Equals(ChannelValues["io,di-ul2-719"]) ? "2" : "3";
            Params["lsw2_qs_off"] = ("1").Equals(ChannelValues["io,di-ul2-710"]) ? "4" : "3";
            Params["lsw2_qf_off"] = ("1").Equals(ChannelValues["io,di-ul2-712"]) ? "4" : "3";
            Params["lsw2_600v_fault"] = ("1").Equals(ChannelValues["io,di-ul2-714"]) ? "5" : "3";
            Params["lsw2_short_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul2-716"]) ? "5" : "2";
            Params["lsw2_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul2-n01"]) ? "5" : "2";
            Params["lsw3_tc_on"] = ("1").Equals(ChannelValues["io,di-ul3-708"]) ? "2" : "3";
            Params["lsw3_spare_switch_on"] = ("1").Equals(ChannelValues["io,di-ul3-719"]) ? "2" : "3";
            Params["lsw3_qs_off"] = ("1").Equals(ChannelValues["io,di-ul3-710"]) ? "4" : "3";
            Params["lsw3_qf_off"] = ("1").Equals(ChannelValues["io,di-ul3-712"]) ? "4" : "3";
            Params["lsw3_600v_fault"] = ("1").Equals(ChannelValues["io,di-ul3-714"]) ? "5" : "3";
            Params["lsw3_short_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul3-716"]) ? "5" : "2";
            Params["lsw3_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul3-n01"]) ? "5" : "2";
            Params["lsw4_tc_on"] = ("1").Equals(ChannelValues["io,di-ul4-708"]) ? "2" : "3";
            Params["lsw4_spare_switch_on"] = ("1").Equals(ChannelValues["io,di-ul4-719"]) ? "2" : "3";
            Params["lsw4_qs_off"] = ("1").Equals(ChannelValues["io,di-ul4-710"]) ? "4" : "3";
            Params["lsw4_qf_off"] = ("1").Equals(ChannelValues["io,di-ul4-712"]) ? "4" : "3";
            Params["lsw4_600v_fault"] = ("1").Equals(ChannelValues["io,di-ul4-714"]) ? "5" : "3";
            Params["lsw4_short_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul4-716"]) ? "5" : "2";
            Params["lsw4_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul4-n01"]) ? "5" : "2";
            Params["lsw5_tc_on"] = ("1").Equals(ChannelValues["io,di-ul5-708"]) ? "2" : "3";
            Params["lsw5_spare_switch_on"] = ("1").Equals(ChannelValues["io,di-ul5-719"]) ? "2" : "3";
            Params["lsw5_qs_off"] = ("1").Equals(ChannelValues["io,di-ul5-710"]) ? "4" : "3";
            Params["lsw5_qf_off"] = ("1").Equals(ChannelValues["io,di-ul5-712"]) ? "4" : "3";
            Params["lsw5_600v_fault"] = ("1").Equals(ChannelValues["io,di-ul5-714"]) ? "5" : "3";
            Params["lsw5_short_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul5-716"]) ? "5" : "2";
            Params["lsw5_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ul5-n01"]) ? "5" : "2";
            Params["lsw9_tc_on"] = ("1").Equals(ChannelValues["io,di-zap-708"]) ? "2" : "3";
            Params["lsw9_qs_off"] = ("1").Equals(ChannelValues["io,di-zap-710"]) ? "4" : "3";
            Params["lsw9_qf_off"] = ("1").Equals(ChannelValues["io,di-zap-712"]) ? "4" : "3"; ;
            Params["lsw9_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-zap-n01"]) ? "5" : "2";

            if (("2").Equals(Params["lsw1_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw1_600v_fault"]) || ("5").Equals(Params["lsw1_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw1_control_circuit_fault"]) && ("3").Equals(Params["lsw1_qf_off"])))
                {
                    Params["lsw1_state"] = "9";
                }
                else if (("3").Equals(Params["lsw1_qf_off"]) && ("3").Equals(Params["lsw1_qs_off"]))
                {
                    Params["lsw1_state"] = "7";
                }
                else
                {
                    Params["lsw1_state"] = "8";
                }
            }
            else if (("3").Equals(Params["lsw1_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw1_600v_fault"]) || ("5").Equals(Params["lsw1_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw1_control_circuit_fault"]) && ("3").Equals(Params["lsw1_qf_off"])))
                {
                    Params["lsw1_state"] = "13";
                }
                else if (("3").Equals(Params["lsw1_qf_off"]) && ("3").Equals(Params["lsw1_qs_off"]))
                {
                    Params["lsw1_state"] = "11";
                }
                else
                {
                    Params["lsw1_state"] = "12";
                }
            }
            else
            {
                if (("5").Equals(Params["lsw1_600v_fault"]) || ("5").Equals(Params["lsw1_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw1_control_circuit_fault"]) && ("3").Equals(Params["lsw1_qf_off"])))
                {
                    Params["lsw1_state"] = "5";
                }
                else if (("3").Equals(Params["lsw1_qf_off"]) && ("3").Equals(Params["lsw1_qs_off"]))
                {
                    Params["lsw1_state"] = "3";
                }
                else
                {
                    Params["lsw1_state"] = "4";
                }
            }

            if (("2").Equals(Params["lsw2_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw2_600v_fault"]) || ("5").Equals(Params["lsw2_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw2_control_circuit_fault"]) && ("3").Equals(Params["lsw2_qf_off"])))
                {
                    Params["lsw2_state"] = "9";
                }
                else if (("3").Equals(Params["lsw2_qf_off"]) && ("3").Equals(Params["lsw2_qs_off"]))
                {
                    Params["lsw2_state"] = "7";
                }
                else
                {
                    Params["lsw2_state"] = "8";
                }
            }
            else if (("3").Equals(Params["lsw2_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw2_600v_fault"]) || ("5").Equals(Params["lsw2_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw2_control_circuit_fault"]) && ("3").Equals(Params["lsw2_qf_off"])))
                {
                    Params["lsw2_state"] = "13";
                }
                else if (("3").Equals(Params["lsw2_qf_off"]) && ("3").Equals(Params["lsw2_qs_off"]))
                {
                    Params["lsw2_state"] = "11";
                }
                else
                {
                    Params["lsw2_state"] = "12";
                }
            }
            else
            {
                if (("5").Equals(Params["lsw2_600v_fault"]) || ("5").Equals(Params["lsw2_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw2_control_circuit_fault"]) && ("3").Equals(Params["lsw2_qf_off"])))
                {
                    Params["lsw2_state"] = "5";
                }
                else if (("3").Equals(Params["lsw2_qf_off"]) && ("3").Equals(Params["lsw2_qs_off"]))
                {
                    Params["lsw2_state"] = "3";
                }
                else
                {
                    Params["lsw2_state"] = "4";
                }
            }

            if (("2").Equals(Params["lsw3_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw3_600v_fault"]) || ("5").Equals(Params["lsw3_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw3_control_circuit_fault"]) && ("3").Equals(Params["lsw3_qf_off"])))
                {
                    Params["lsw3_state"] = "9";
                }
                else if (("3").Equals(Params["lsw3_qf_off"]) && ("3").Equals(Params["lsw3_qs_off"]))
                {
                    Params["lsw3_state"] = "7";
                }
                else
                {
                    Params["lsw3_state"] = "8";
                }
            }
            else if (("3").Equals(Params["lsw3_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw3_600v_fault"]) || ("5").Equals(Params["lsw3_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw3_control_circuit_fault"]) && ("3").Equals(Params["lsw3_qf_off"])))
                {
                    Params["lsw3_state"] = "13";
                }
                else if (("3").Equals(Params["lsw3_qf_off"]) && ("3").Equals(Params["lsw3_qs_off"]))
                {
                    Params["lsw3_state"] = "11";
                }
                else
                {
                    Params["lsw3_state"] = "12";
                }
            }
            else
            {
                if (("5").Equals(Params["lsw3_600v_fault"]) || ("5").Equals(Params["lsw3_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw3_control_circuit_fault"]) && ("3").Equals(Params["lsw3_qf_off"])))
                {
                    Params["lsw3_state"] = "5";
                }
                else if (("3").Equals(Params["lsw3_qf_off"]) && ("3").Equals(Params["lsw3_qs_off"]))
                {
                    Params["lsw3_state"] = "3";
                }
                else
                {
                    Params["lsw3_state"] = "4";
                }
            }

            if (("2").Equals(Params["lsw4_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw4_600v_fault"]) || ("5").Equals(Params["lsw4_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw4_control_circuit_fault"]) && ("3").Equals(Params["lsw4_qf_off"])))
                {
                    Params["lsw4_state"] = "9";
                }
                else if (("3").Equals(Params["lsw4_qf_off"]) && ("3").Equals(Params["lsw4_qs_off"]))
                {
                    Params["lsw4_state"] = "7";
                }
                else
                {
                    Params["lsw4_state"] = "8";
                }
            }
            else if (("3").Equals(Params["lsw4_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw4_600v_fault"]) || ("5").Equals(Params["lsw4_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw4_control_circuit_fault"]) && ("3").Equals(Params["lsw4_qf_off"])))
                {
                    Params["lsw4_state"] = "13";
                }
                else if (("3").Equals(Params["lsw4_qf_off"]) && ("3").Equals(Params["lsw4_qs_off"]))
                {
                    Params["lsw4_state"] = "11";
                }
                else
                {
                    Params["lsw4_state"] = "12";
                }
            }
            else
            {
                if (("5").Equals(Params["lsw4_600v_fault"]) || ("5").Equals(Params["lsw4_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw4_control_circuit_fault"]) && ("3").Equals(Params["lsw4_qf_off"])))
                {
                    Params["lsw4_state"] = "5";
                }
                else if (("3").Equals(Params["lsw4_qf_off"]) && ("3").Equals(Params["lsw4_qs_off"]))
                {
                    Params["lsw4_state"] = "3";
                }
                else
                {
                    Params["lsw4_state"] = "4";
                }
            }

            if (("2").Equals(Params["lsw5_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw5_600v_fault"]) || ("5").Equals(Params["lsw5_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw5_control_circuit_fault"]) && ("3").Equals(Params["lsw5_qf_off"])))
                {
                    Params["lsw5_state"] = "9";
                }
                else if (("3").Equals(Params["lsw5_qf_off"]) && ("3").Equals(Params["lsw5_qs_off"]))
                {
                    Params["lsw5_state"] = "7";
                }
                else
                {
                    Params["lsw5_state"] = "8";
                }
            }
            else if (("3").Equals(Params["lsw5_spare_switch_on"]))
            {
                if (("5").Equals(Params["lsw5_600v_fault"]) || ("5").Equals(Params["lsw5_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw5_control_circuit_fault"]) && ("3").Equals(Params["lsw5_qf_off"])))
                {
                    Params["lsw5_state"] = "13";
                }
                else if (("3").Equals(Params["lsw5_qf_off"]) && ("3").Equals(Params["lsw5_qs_off"]))
                {
                    Params["lsw5_state"] = "11";
                }
                else
                {
                    Params["lsw5_state"] = "12";
                }
            }
            else
            {
                if (("5").Equals(Params["lsw5_600v_fault"]) || ("5").Equals(Params["lsw5_short_circuit_fault"]) ||
                    (("5").Equals(Params["lsw5_control_circuit_fault"]) && ("3").Equals(Params["lsw5_qf_off"])))
                {
                    Params["lsw5_state"] = "5";
                }
                else if (("3").Equals(Params["lsw5_qf_off"]) && ("3").Equals(Params["lsw5_qs_off"]))
                {
                    Params["lsw5_state"] = "3";
                }
                else
                {
                    Params["lsw5_state"] = "4";
                }
            }

            if (("5").Equals(Params["lsw9_control_circuit_fault"]))
            {
                Params["lsw9_state"] = "5";
            }
            else if (("3").Equals(Params["lsw9_qf_off"]) && ("3").Equals(Params["lsw9_qs_off"]))
            {
                Params["lsw9_state"] = "3";
            }
            else
            {
                Params["lsw9_state"] = "4";
            }

            Params["general_sn_auto_on"] = ("1").Equals(ChannelValues["io,di-sn-238"]) ? "3" : "4";
            Params["general_sn_leadin1_contacts_fault"] = ("1").Equals(ChannelValues["io,di-sn-242"]) ? "3" : "2";
            Params["general_sn_leadin2_contacts_fault"] = ("1").Equals(ChannelValues["io,di-sn-244"]) ? "3" : "2";
            Params["general_sn_24v_on"] = ("1").Equals(ChannelValues["io,di-sn-339"]) ? "5" : "2";
            Params["general_fire_alarm"] = ("1").Equals(ChannelValues["io,di-111"]) ? "5" : "2";
            Params["general_intrusion_alarm"] = ("1").Equals(ChannelValues["io,di-113"]) ? "4" : "2";
            Params["general_ol_on"] = ("1").Equals(ChannelValues["io,di-ol-908"]) ? "3" : "2";
            Params["general_ol_tc_on"] = ("1").Equals(ChannelValues["io,di-ol-916"]) ? "3" : "2";
            Params["general_ol_fault"] = ("1").Equals(ChannelValues["io,di-ol-910"]) ? "5" : "2";
            Params["general_ol_control_circuit_fault"] = ("1").Equals(ChannelValues["io,di-ol-912"]) ? "5" : "2"

            if (ProcessingUpdateEvent != null)
            {
                ProcessingUpdateEvent();
            }
        }*/
    }
}

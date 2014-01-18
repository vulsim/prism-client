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
        private int QueryChannelCounter;
        private int MaxQueryChannelCounter;

        public Dictionary<string, Alarm> AlarmValues;
        public Dictionary<string, string> ChannelValues;
        public Dictionary<string, Param> Params;

        public event ProcessingUpdateEventHandler ProcessingUpdateEvent;
        public event ProcessingChangeStateEventHandler ProcessingChangeStateEvent;

        public bool IsAvaliable { get { return IsQueryAvaliable; } }
        public bool IsBusy { get { return IsAutoUpdateInProgress || (OperateRetainCount > 0); } }

        public Processing(UnitSettings settings)
        {
            AlarmValues = new Dictionary<string, Alarm>();
            ChannelValues = new Dictionary<string, string>();
            Params = new Dictionary<string, Param>();

            Params["leadin1_state_in_switch"] = new Param("leadin1_state_in_switch", ChannelValues, "io,di-rab-908", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["leadin1_state_tc_switch"] = new Param("leadin1_state_tc_switch", ChannelValues, "io,di-rab-916", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "1"), new ParamMapValue(ParamState.B, "0") }));
            Params["leadin1_alarm_in_switch_fault"] = new Param("leadin1_alarm_in_switch_fault", ChannelValues, "io,di-rab-910", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin1_alarm_circuit_fault"] = new Param("leadin1_alarm_circuit_fault", ChannelValues, "io,di-rab-912", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin1_alarm_tn_circuit_fault"] = new Param("leadin1_alarm_tn_circuit_fault", ChannelValues, "io,di-tn1-918", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin1_alarm_tn_ru6kv_fault"] = new Param("leadin1_alarm_tn_ru6kv_fault", ChannelValues, "io,di-tn1-920", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin1_alarm_tsn_lost_power"] = new Param("leadin1_alarm_tsn_lost_power", ChannelValues, "io,di-tsn1-ts71", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["leadin2_state_in_switch"] = new Param("leadin2_state_in_switch", ChannelValues, "io,di-rez-900", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["leadin2_state_tc_switch"] = new Param("leadin2_state_tc_switch", ChannelValues, "io,di-rez-906", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["leadin2_alarm_in_switch_fault"] = new Param("leadin2_alarm_in_switch_fault", ChannelValues, "io,di-rez-902", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin2_alarm_circuit_fault"] = new Param("leadin1_alarm_circuit_fault", ChannelValues, "io,di-rez-904", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin2_alarm_tn_circuit_fault"] = new Param("leadin1_alarm_tn_circuit_fault", ChannelValues, "io,di-tn2-918", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin2_alarm_tn_ru6kv_fault"] = new Param("leadin1_alarm_tn_ru6kv_fault", ChannelValues, "io,di-tn2-741", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["leadin2_alarm_tsn_lost_power"] = new Param("leadin1_alarm_tsn_lost_power", ChannelValues, "io,di-tsn2-ts71", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["ol_state_in_switch"] = new Param("ol_state_in_switch", ChannelValues, "io,di-ol-908", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["ol_state_tc_switch"] = new Param("ol_state_tc_switch", ChannelValues, "io,di-ol-916", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "1"), new ParamMapValue(ParamState.B, "0") }));
            Params["ol_alarm_switch_fault"] = new Param("ol_alarm_switch_fault", ChannelValues, "io,di-ol-910", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["ol_alarm_circuit_fault"] = new Param("ol_alarm_circuit_fault", ChannelValues, "io,di-ol-912", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

            Params["ol_state"] = new Param("ol_state", new List<ParamRelation> 
            { 
                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["ol_state_in_switch"], ParamState.A), 
                    new ParamCombination(Params["ol_alarm_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["ol_alarm_circuit_fault"], ParamState.Idle)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["ol_state_in_switch"], ParamState.B), 
                    new ParamCombination(Params["ol_alarm_switch_fault"], ParamState.Idle),
                    new ParamCombination(Params["ol_alarm_circuit_fault"], ParamState.Idle)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.C)
            });

            Params["rect1_state_pa_switch"] = new Param("rect1_state_pa_switch", ChannelValues, "io,di-pa1-911", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["rect1_state_qs_switch"] = new Param("rect1_state_qs_switch", ChannelValues, "io,di-ru1-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect1_state_qf_switch"] = new Param("rect1_state_qf_switch", ChannelValues, "io,di-ru1-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect1_state_tc_switch"] = new Param("rect1_state_tc_switch", ChannelValues, "io,di-ru1-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect1_alarm_circuit_fault"] = new Param("rect1_alarm_circuit_fault", ChannelValues, "io,di-ka1-n02", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_pa_switch_fault"] = new Param("rect1_alarm_pa_switch_fault", ChannelValues, "io,di-pa1-1003", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_rec_fault"] = new Param("rect1_alarm_rec_fault", ChannelValues, "io,di-v1-67", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_rec_gas_warn"] = new Param("rect1_alarm_rec_gas_warn", ChannelValues, "io,di-v1-86", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_rec_overload"] = new Param("rect1_alarm_rec_overload", ChannelValues, "io,di-v1-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect1_alarm_rec_rpz600v_fault"] = new Param("rect1_alarm_rec_rpz600v_fault", ChannelValues, "io,di-v1-106", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["rect2_state_pa_switch"] = new Param("rect2_state_pa_switch", ChannelValues, "io,di-pa2-911", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["rect2_state_qs_switch"] = new Param("rect2_state_qs_switch", ChannelValues, "io,di-ru2-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect2_state_qf_switch"] = new Param("rect2_state_qf_switch", ChannelValues, "io,di-ru2-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect2_state_tc_switch"] = new Param("rect2_state_tc_switch", ChannelValues, "io,di-ru2-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect2_alarm_circuit_fault"] = new Param("rect2_alarm_circuit_fault", ChannelValues, "io,di-ka2-n02", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_pa_switch_fault"] = new Param("rect2_alarm_pa_switch_fault", ChannelValues, "io,di-pa2-1003", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_rec_fault"] = new Param("rect2_alarm_rec_fault", ChannelValues, "io,di-v2-67", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_rec_gas_warn"] = new Param("rect2_alarm_rec_gas_warn", ChannelValues, "io,di-v2-86", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_rec_overload"] = new Param("rect2_alarm_rec_overload", ChannelValues, "io,di-v2-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect2_alarm_rec_rpz600v_fault"] = new Param("rect2_alarm_rec_rpz600v_fault", ChannelValues, "io,di-v2-106", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["rect3_state_pa_switch"] = new Param("rect3_state_pa_switch", ChannelValues, "io,di-pa3-911", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.B, "0"), new ParamMapValue(ParamState.A, "1") }));
            Params["rect3_state_qs_switch"] = new Param("rect3_state_qs_switch", ChannelValues, "io,di-ru3-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect3_state_qf_switch"] = new Param("rect3_state_qf_switch", ChannelValues, "io,di-ru3-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect3_state_tc_switch"] = new Param("rect3_state_tc_switch", ChannelValues, "io,di-ru3-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["rect3_alarm_circuit_fault"] = new Param("rect3_alarm_circuit_fault", ChannelValues, "io,di-ka3-n02", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_pa_switch_fault"] = new Param("rect3_alarm_pa_switch_fault", ChannelValues, "io,di-pa3-1003", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_rec_fault"] = new Param("rect3_alarm_rec_fault", ChannelValues, "io,di-v3-67", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_rec_gas_warn"] = new Param("rect3_alarm_rec_gas_warn", ChannelValues, "io,di-v3-86", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_rec_overload"] = new Param("rect3_alarm_rec_overload", ChannelValues, "io,di-v3-111", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["rect3_alarm_rec_rpz600v_fault"] = new Param("rect3_alarm_rec_rpz600v_fault", ChannelValues, "io,di-v3-106", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["lsw1_state_qs_switch"] = new Param("lsw1_state_qs_switch", ChannelValues, "io,di-ul1-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw1_state_qf_switch"] = new Param("lsw1_state_qf_switch", ChannelValues, "io,di-ul1-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw1_state_tc_switch"] = new Param("lsw1_state_tc_switch", ChannelValues, "io,di-ul1-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw1_state_spare_switch"] = new Param("lsw1_state_spare_switch", ChannelValues, "io,di-ul1-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw1_alarm_short_fault"] = new Param("lsw1_alarm_short_fault", ChannelValues, "io,di-ul1-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw1_alarm_circuit_fault"] = new Param("lsw1_alarm_circuit_fault", ChannelValues, "io,di-ul1-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw1_alarm_600v_lost_power"] = new Param("lsw1_alarm_600v_lost_power", ChannelValues, "io,di-ul1-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["lsw2_state_qs_switch"] = new Param("lsw2_state_qs_switch", ChannelValues, "io,di-ul2-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw2_state_qf_switch"] = new Param("lsw2_state_qf_switch", ChannelValues, "io,di-ul2-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw2_state_tc_switch"] = new Param("lsw2_state_tc_switch", ChannelValues, "io,di-ul2-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw2_state_spare_switch"] = new Param("lsw2_state_spare_switch", ChannelValues, "io,di-ul2-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw2_alarm_short_fault"] = new Param("lsw2_alarm_short_fault", ChannelValues, "io,di-ul2-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw2_alarm_circuit_fault"] = new Param("lsw2_alarm_circuit_fault", ChannelValues, "io,di-ul2-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw2_alarm_600v_lost_power"] = new Param("lsw2_alarm_600v_lost_power", ChannelValues, "io,di-ul2-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["lsw3_state_qs_switch"] = new Param("lsw3_state_qs_switch", ChannelValues, "io,di-ul3-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw3_state_qf_switch"] = new Param("lsw3_state_qf_switch", ChannelValues, "io,di-ul3-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw3_state_tc_switch"] = new Param("lsw3_state_tc_switch", ChannelValues, "io,di-ul3-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw3_state_spare_switch"] = new Param("lsw3_state_spare_switch", ChannelValues, "io,di-ul3-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw3_alarm_short_fault"] = new Param("lsw3_alarm_short_fault", ChannelValues, "io,di-ul3-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw3_alarm_circuit_fault"] = new Param("lsw3_alarm_circuit_fault", ChannelValues, "io,di-ul3-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw3_alarm_600v_lost_power"] = new Param("lsw3_alarm_600v_lost_power", ChannelValues, "io,di-ul3-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["lsw4_state_qs_switch"] = new Param("lsw4_state_qs_switch", ChannelValues, "io,di-ul4-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw4_state_qf_switch"] = new Param("lsw4_state_qf_switch", ChannelValues, "io,di-ul4-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw4_state_tc_switch"] = new Param("lsw4_state_tc_switch", ChannelValues, "io,di-ul4-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw4_state_spare_switch"] = new Param("lsw4_state_spare_switch", ChannelValues, "io,di-ul4-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw4_alarm_short_fault"] = new Param("lsw4_alarm_short_fault", ChannelValues, "io,di-ul4-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw4_alarm_circuit_fault"] = new Param("lsw4_alarm_circuit_fault", ChannelValues, "io,di-ul4-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw4_alarm_600v_lost_power"] = new Param("lsw4_alarm_600v_lost_power", ChannelValues, "io,di-ul4-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["lsw5_state_qs_switch"] = new Param("lsw5_state_qs_switch", ChannelValues, "io,di-ul5-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw5_state_qf_switch"] = new Param("lsw5_state_qf_switch", ChannelValues, "io,di-ul5-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw5_state_tc_switch"] = new Param("lsw5_state_tc_switch", ChannelValues, "io,di-ul5-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw5_state_spare_switch"] = new Param("lsw5_state_spare_switch", ChannelValues, "io,di-ul5-719", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw5_alarm_short_fault"] = new Param("lsw5_alarm_short_fault", ChannelValues, "io,di-ul5-716", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw5_alarm_circuit_fault"] = new Param("lsw5_alarm_circuit_fault", ChannelValues, "io,di-ul5-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));
            Params["lsw5_alarm_600v_lost_power"] = new Param("lsw5_alarm_600v_lost_power", ChannelValues, "io,di-ul5-714", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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

            Params["lsw9_state_qs_switch"] = new Param("lsw9_state_qs_switch", ChannelValues, "io,di-zap-710", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw9_state_qf_switch"] = new Param("lsw9_state_qf_switch", ChannelValues, "io,di-zap-712", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw9_state_tc_switch"] = new Param("lsw9_state_tc_switch", ChannelValues, "io,di-zap-708", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.A, "0"), new ParamMapValue(ParamState.B, "1") }));
            Params["lsw9_alarm_circuit_fault"] = new Param("lsw9_alarm_circuit_fault", ChannelValues, "io,di-zap-n01", new ParamMap(new List<ParamMapValue> { new ParamMapValue(ParamState.Idle, "0"), new ParamMapValue(ParamState.C, "1") }));

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
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.B),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.B),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.B),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.B),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.A),*/
                    new ParamCombination(Params["lsw9_state"], ParamState.A)
                }, ParamState.B),

                new ParamRelation(new List<ParamCombination> 
                { 
                    new ParamCombination(Params["lsw1_state"], ParamState.A),
                    new ParamCombination(Params["lsw2_state"], ParamState.A),
                    new ParamCombination(Params["lsw3_state"], ParamState.A),
                    new ParamCombination(Params["lsw4_state"], ParamState.A),
                    /*new ParamCombination(Params["lsw5_state"], ParamState.B),*/
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
                    new ParamCombination(Params["common_group2_state"], ParamState.A),
                    new ParamCombination(Params["common_group3_state"], ParamState.A),
                    new ParamCombination(Params["common_group4_state"], ParamState.A)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group1_state"], ParamState.C)
                }, ParamState.C),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group2_state"], ParamState.C)
                }, ParamState.C),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group3_state"], ParamState.C)
                }, ParamState.C),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group4_state"], ParamState.C)
                }, ParamState.C),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group2_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                {
                    new ParamCombination(Params["common_group3_state"], ParamState.B)
                }, ParamState.A),

                new ParamRelation(new List<ParamCombination> 
                { 
                    
                }, ParamState.B)
            });

            Params["leadin1_instant_current"] = new Param("leadin1_instant_current", ChannelValues, "io,leadin1_instant_current");
            Params["leadin1_total_active_energy"] = new Param("leadin1_total_active_energy", ChannelValues, "io,leadin1_total_active_energy");
            Params["leadin2_instant_current"] = new Param("leadin2_instant_current", ChannelValues, "io,leadin2_instant_current");
            Params["leadin2_total_active_energy"] = new Param("leadin2_total_active_energy", ChannelValues, "io,leadin2_total_active_energy");
            Params["ol_instant_current"] = new Param("ol_instant_current", ChannelValues, "io,ol_instant_current");
            Params["ol_total_active_energy"] = new Param("ol_total_active_energy", ChannelValues, "io,ol_total_active_energy");

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
            producerSettings.Channels.Add(new ProducerChannel("io", "leadin1_instant_current"));
            producerSettings.Channels.Add(new ProducerChannel("io", "leadin1_total_active_energy"));
            producerSettings.Channels.Add(new ProducerChannel("io", "leadin2_instant_current"));
            producerSettings.Channels.Add(new ProducerChannel("io", "leadin2_total_active_energy"));
            producerSettings.Channels.Add(new ProducerChannel("io", "ol_instant_current"));
            producerSettings.Channels.Add(new ProducerChannel("io", "ol_total_active_energy"));            

            producer = new Producer(producerSettings);
            producer.ChannelValueEvent += ProducerChannelValueEvent;
            producer.ChannelResetEvent += ProducerChannelResetEvent;
            producer.Start();

            autoUpdateTimer = new System.Timers.Timer(180000);
            autoUpdateTimer.Elapsed += ProducerAutoUpdateEvent;

            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                ProducerAutoUpdateEvent(null, null);
                autoUpdateTimer.Start();
            }, null);
        }

        ~Processing()
        {
            autoUpdateTimer.Stop();
            producer.ChannelValueEvent -= ProducerChannelValueEvent;
            producer.ChannelResetEvent -= ProducerChannelResetEvent;
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
            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                if (ProcessingChangeStateEvent != null)
                {
                    ProcessingChangeStateEvent(this);
                }
            }, null);

            ManualResetEvent continueEvent = new ManualResetEvent(false);
            System.Timers.Timer queryUnvaliableTimer = new System.Timers.Timer(30000);

            queryUnvaliableTimer.Elapsed += delegate(object s, ElapsedEventArgs ev)
            {
                IsQueryAvaliable = false;
                queryUnvaliableTimer.Stop();
                queryUnvaliableTimer = null;
                continueEvent.Set();
            };
            queryUnvaliableTimer.Start();

            this.MaxQueryChannelCounter = producerSettings.Channels.Count;
            this.QueryChannelCounter = producerSettings.Channels.Count;

            producer.GetChannelList("alarm", delegate(string error, List<ProducerChannel> channels)
            {
                if (error == null)
                {
                    foreach (var channel in channels)
                    {
                        if (!channel.Channel.Contains("-manual"))
                        {
                            this.MaxQueryChannelCounter++;
                            this.QueryChannelCounter++;
                        }
                    }                    
                    AlarmValues.Clear();

                    foreach (var channel in channels)
                    {
                        if (!channel.Channel.Contains("-manual"))
                        {
                            AlarmValues[channel.Group + "," + channel.Channel] = new Alarm(channel.Channel, "", ParamState.Unknown);

                            producer.ReadChannelValue(channel, delegate(string error1, ProducerChannelValue value)
                            {
                                if (error1 == null)
                                {
                                    AlarmValues[value.Group + "," + value.Channel] = this.AlarmFromChannelValue(value);
                                }

                                this.QueryChannelCounter--;
                                if (this.QueryChannelCounter == 0)
                                {
                                    continueEvent.Set();
                                }
                            });
                        }
                    }
                }

                foreach (var channel in producerSettings.Channels)
                {
                    producer.ReadChannelValue(channel, delegate(string error2, ProducerChannelValue value)
                    {                       
                        if (error == null && value != null)
                        {
                            ChannelValues[value.Group + "," + value.Channel] = value.Value;
                            this.QueryChannelCounter--;
                        }
                        else
                        {
                            try
                            {
                                ChannelValues.Remove(value.Group + "," + value.Channel);
                            }
                            catch (SystemException err)
                            {

                            }
                        }
                        
                        if (this.QueryChannelCounter == 0)
                        {
                            continueEvent.Set();
                        }
                    });
                }
            });
            continueEvent.WaitOne();

            if (this.QueryChannelCounter < this.MaxQueryChannelCounter)
            {
                if (this.QueryChannelCounter == 0)
                {
                    if (autoUpdateTimer.Interval != 90000)
                    {
                        autoUpdateTimer.Interval = 90000;
                    }
                }
                else if (autoUpdateTimer.Interval != 5000)
                {
                    autoUpdateTimer.Interval = 5000;
                }

                if (queryUnvaliableTimer != null)
                {
                    queryUnvaliableTimer.Stop();
                }

                IsQueryAvaliable = true;

                ThreadPool.QueueUserWorkItem(delegate(object target)
                {
                    if (ProcessingUpdateEvent != null)
                    {
                        ProcessingUpdateEvent(this);
                    }
                }, null);
            }
            else
            {
                producer.ChannelValueEvent -= ProducerChannelValueEvent;
                producer.ChannelResetEvent -= ProducerChannelResetEvent;

                producer = new Producer(producerSettings);
                producer.ChannelValueEvent += ProducerChannelValueEvent;
                producer.ChannelResetEvent += ProducerChannelResetEvent;
                producer.Start();

                autoUpdateTimer.Interval = 1000;
            }

            IsAutoUpdateInProgress = false;
            ThreadPool.QueueUserWorkItem(delegate(object target)
            {
                if (ProcessingChangeStateEvent != null)
                {
                    ProcessingChangeStateEvent(this);
                }
            }, null);
        }

        private void ProducerChannelValueEvent(object sender, ProducerChannelValue value)
        {
            if (value.Group.Equals("alarm"))
            {
                if (!value.Channel.Contains("-manual"))
                {
                    AlarmValues["alarm," + value.Channel] = this.AlarmFromChannelValue(value);

                    ThreadPool.QueueUserWorkItem(delegate(object target)
                    {
                        if (ProcessingUpdateEvent != null)
                        {
                            ProcessingUpdateEvent(this);
                        }
                    }, null);
                }
            }
            else
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
        }

        void ProducerChannelResetEvent(object sender, ProducerChannel channel)
        {
            if (channel.Group.Equals("alarm"))
            {
                if (!channel.Channel.Contains("-manual"))
                {
                    try
                    {
                        AlarmValues.Remove("alarm," + channel.Channel);
                    }
                    catch (SystemException e)
                    {
                    }
                    
                    ThreadPool.QueueUserWorkItem(delegate(object target)
                    {
                        if (ProcessingUpdateEvent != null)
                        {
                            ProcessingUpdateEvent(this);
                        }
                    }, null);
                }
            }
            else
            {
                try
                {
                    ChannelValues.Remove(channel.Group + "," + channel.Channel);
                }
                catch (SystemException e)
                {
                }

                ThreadPool.QueueUserWorkItem(delegate(object target)
                {
                    if (ProcessingUpdateEvent != null)
                    {
                        ProcessingUpdateEvent(this);
                    }
                }, null);
            }
        }

        private Alarm AlarmFromChannelValue(ProducerChannelValue value)
        {
            if (value.Group.Equals("alarm"))
            {
                string[] splitValue = value.Value.Split(" ".ToCharArray(), 2);

                if (splitValue != null && splitValue.Length == 2)
                {
                    ParamState paramState = ParamState.Unknown;

                    if (splitValue[0].Equals("[I]"))
                    {
                        paramState = ParamState.Idle;
                    }
                    else if (splitValue[0].Equals("[A]"))
                    {
                        paramState = ParamState.A;
                    }
                    else if (splitValue[0].Equals("[B]"))
                    {
                        paramState = ParamState.B;
                    }
                    if (splitValue[0].Equals("[C]"))
                    {
                        paramState = ParamState.C;
                    }

                    return new Alarm(value.Channel, splitValue[1], paramState);
                }
            }

            return null;
        }
    }
}

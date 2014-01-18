using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prism.General.Automation
{
    public enum ParamState : uint
    {
        Unknown = 0,
        Idle = 1,
        A = 2,
        B = 3,
        C = 4
    }

    public class ParamCombination
    {
        public Param Param { get; set; }
        public ParamState State { get; set; }

        public ParamCombination(Param param, ParamState state)
        {
            this.Param = param;
            this.State = state;
        }
    }

    public class ParamRelation
    {
        public List<ParamCombination> Combinations { get; set; }
        public ParamState State { get; set; }
        public bool Match
        {
            get
            {
                bool match = true;

                foreach (var combination in Combinations)
                {
                    if (combination.Param.State != combination.State)
                    {
                        match = false;
                        break;
                    }
                }

                return match;
            }
        }

        public ParamRelation(List<ParamCombination> combinations, ParamState state)
        {
            this.Combinations = combinations;
            this.State = state;
        }
    }

    public class ParamMapValue
    {
        public ParamState State { get; set; }
        public string Value { get; set; }
        public ParamMapValue(ParamState state, string value)
        {
            this.State = state;
            this.Value = value;
        }
    }

    public class ParamMap
    {
        public List<ParamMapValue> Map { get; set; }

        public ParamMap(List<ParamMapValue> map)
        {
            this.Map = map;
        }

        public ParamState GetState(string value)
        {
            if (value != null)
            {
                foreach (var mapValue in Map)
                {
                    if (value.Equals(mapValue.Value))
                    {
                        return mapValue.State;
                    }
                }
            }

            return ParamState.Unknown;
        }
    }

    public class Param
    {
        public enum ParamAssignType { Value, Map, Relation }
        public string Name { get { return NameInternal; } }

        public ParamState State
        {
            get
            {
                ParamState paramState = ParamState.Unknown;

                try
                {
                    if (AssignType == ParamAssignType.Map)
                    {
                        paramState = Map.GetState(Store[Key]);
                    }
                    else if (AssignType == ParamAssignType.Relation)
                    {
                        foreach (var relation in Relations)
                        {
                            if (relation.Match)
                            {
                                paramState = relation.State;
                                break;
                            }
                        }
                    }
                }
                catch (SystemException e)
                {

                }

                return paramState;
            }
        }

        public string Value
        {
            get
            {
                string value = null;

                try
                {
                    if (AssignType == ParamAssignType.Value)
                    {
                        value = Store[Key];
                    }                    
                }
                catch (SystemException e)
                {

                }

                return value;
            }
        }

        public ParamAssignType Type
        {
            get
            {
                return AssignType;
            }
        }

        private string NameInternal;

        private ParamAssignType AssignType;
        private Dictionary<string, string> Store;
        private List<ParamRelation> Relations;
        private ParamMap Map;
        private string Key;

        public Param(string name, List<ParamRelation> relations)
        {
            this.AssignType = ParamAssignType.Relation;
            this.NameInternal = name;
            this.Relations = relations;
        }

        public Param(string name, Dictionary<string, string> store, string key)
        {
            this.AssignType = ParamAssignType.Value;
            this.NameInternal = name;
            this.Store = store;
            this.Key = key;
        }

        public Param(string name, Dictionary<string, string> store, string key, ParamMap map)
        {
            this.AssignType = ParamAssignType.Map;
            this.NameInternal = name;
            this.Store = store;
            this.Key = key;
            this.Map = map;
        }
    }
}

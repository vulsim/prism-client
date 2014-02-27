using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Prism.General
{
    public class UnitSettings
    {
        [JsonProperty("unit")]
        public string FilePath;

        [JsonProperty("symbolic_name")]
        public string SymbolicName;

        [JsonProperty("short_name")]
        public string ShortName;

        [JsonProperty("full_name")]
        public string FullName;

        [JsonProperty("address")]
        public string Address;

        [JsonProperty("common_endpoint")]
        public string CommonEndpoint;

        [JsonProperty("poll_endpoint")]
        public string PollEndpoint;

        [JsonProperty("operate_endpoint")]
        public string OperateEndpoint;

        [JsonProperty("subscribe_endpoint")]
        public string SubscribeEndpoint;
    }
}

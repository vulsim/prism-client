using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Prism.General
{
    public class UnitConnectionSettings
    {
        [JsonProperty("req")]
        public string ReqAddr;

        [JsonProperty("sub")]
        public string SubAddr;
    }

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

        [JsonProperty("connection")]
        public UnitConnectionSettings Connection;
    }
}

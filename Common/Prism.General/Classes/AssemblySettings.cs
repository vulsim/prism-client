using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace Prism.General
{
    public class AssemblySettings
    {
        [JsonProperty("units")]
        public List<UnitSettings> Units;

        public static AssemblySettings Load(string filePath)
        {
            StreamReader settingsReader = new StreamReader(filePath);
            AssemblySettings assemblySettings = JsonConvert.DeserializeObject<AssemblySettings>(settingsReader.ReadToEnd());
            settingsReader.Close();

            return assemblySettings;
        }
    }
}

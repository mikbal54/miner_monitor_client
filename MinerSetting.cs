using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace MinerMonitorClient
{
    public class MinerSetting
    {
        public static string minerPath = "";
        public string parameters = "";
        public string profile_name = "";
        public Process process;
        virtual public void Start() { }

        virtual public void ReadMinerStats() { }

        public JObject lastStats;
    }
}

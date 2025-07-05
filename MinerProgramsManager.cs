using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MinerMonitorClient
{
    public sealed class MinerProgramsManager
    {
        private static readonly MinerProgramsManager instance = new MinerProgramsManager();

        public List<MinerSetting> minerSettings;

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public string GetHashRate(int minerIndex = 0)
        {
            string hashrates = minerSettings[minerIndex].lastStats["result"][3].ToString();
            var totals = hashrates.Split(';');
            var total = 0;
            foreach (var s in totals)
                total += int.Parse(s);
            return total.ToString();
        }

        private MinerProgramsManager()
        {
            minerSettings = new List<MinerSetting>();

            // fill miner settings
            JArray miners = Program.config["miners"];

            foreach (JObject miner in miners)
            {
                if (miner["type"].ToString() == "claymore-ethash")
                {
                    ClaymoreEthereumMinerSetting claymoreEth = new ClaymoreEthereumMinerSetting();
                    claymoreEth.parameters = miner["parameters"].ToString();
                    claymoreEth.profile_name = miner["profile_name"].ToString();
                    Regex regex = new Regex(@"(?!%)[a-zA-Z0-9]*(?=%)");
                    MatchCollection matches = regex.Matches(claymoreEth.parameters);
                   
                    foreach(Match match in matches)
                    {
                        claymoreEth.parameters = ReplaceFirst(claymoreEth.parameters, "%" + match.Value + "%", ((JValue)Program.config[match.Value]).ToString() );
                    }

                    minerSettings.Add(claymoreEth);
                }
            }
        }


        public void StartMiners()
        {
            if (minerSettings.Count > 0)
            {
                int index = ((JValue)Program.config["default_profile_index"]).ToObject<int>();

                minerSettings[index].Start();

            }
            else
                Console.WriteLine("Miner did not start! Could not find any miner profiles");
        }

        public void ReadMinerStats()
        {
            foreach(var miner in minerSettings)
            {
                miner.ReadMinerStats();
            }
        }

        public void Run()
        {
            ReadMinerStats();
        }

        public static MinerProgramsManager Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

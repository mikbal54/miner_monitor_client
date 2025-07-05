using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace MinerMonitorClient
{
    public class ClaymoreEthereumMinerSetting :  MinerSetting
    {
        

        public ClaymoreEthereumMinerSetting()
        {
            
        }

        public override void ReadMinerStats()
        {
            var client = new WebClient();
            // TODO: read port from config
            try
            {
                var content = client.DownloadString("http://127.0.0.1:3333");

                Regex regex = new Regex(@"{(.)*}");
                Match match = regex.Match(content);

                lastStats = JsonConvert.DeserializeObject<JObject>(match.ToString());
            }
            catch(Exception e)
            {
                Console.WriteLine("Miner is not working, could not get stats");
            }
        }

        public void End()
        {
            if(process != null)
            {
                process.Close();
                process = null;
            }

            Process[] pname = Process.GetProcessesByName("EthDcrMiner64");
            if(pname.Length > 0)
            {
                pname[0].Close();
            }
        }

        override public void Start()
        {
            End();

            process = new Process();
            process.StartInfo.FileName = "EthDcrMiner64.exe";
            process.StartInfo.WorkingDirectory = "miners\\ClaymoreEthereum";

            process.StartInfo.Arguments += parameters;
             
            Console.WriteLine("Starting Claymore Ethereum Miner...");
            process.Start();
            
        }

    }
}

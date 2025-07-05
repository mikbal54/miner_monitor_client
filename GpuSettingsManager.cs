using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MinerMonitorClient
{
    public sealed class GpuSettingsManager
    {
        private static readonly GpuSettingsManager instance = new GpuSettingsManager();

        public List<IniData> wattToolInis;

        private GpuSettingsManager() {
            wattToolInis = new List<IniData>();
        }

        public void GetWattToolInis()
        {
            
            var parser = new FileIniDataParser();

            int index = 0;
            while (true)
            {
                string file = "apps\\WattTool\\gpu" + index + ".ini";
                if (File.Exists(file))
                {
                    IniData data = parser.ReadFile(file);
                    wattToolInis.Add(data);
                    index++;
                }
                else
                    break;
            }

            Console.WriteLine("Total " + (index) + " WattTool ini files read");
        }
        
        public void ApplyWattToolSettings()
        {
            var p = new Process();
            p.StartInfo.FileName = "WattTool.exe";
            p.StartInfo.WorkingDirectory = "apps\\WattTool";
            for (int i=0; i< wattToolInis.Count; i++)
            {
                p.StartInfo.Arguments += " gpu" + i + ".ini";
            }

            Console.WriteLine("Executing ini files =" + p.StartInfo.Arguments);


            // Disabled for now
            //p.Start();
        }

        public static GpuSettingsManager Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

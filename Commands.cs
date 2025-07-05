using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerMonitorClient
{
    class Commands
    {

        public static void RestartComputer()
        {
            Process.Start("shutdown","/r /t 0");
        }

        public static void ShutdownComputer()
        {
            Process.Start("shutdown", "/s /t 0");
        }

    }
}

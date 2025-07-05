using System.Diagnostics;

namespace MinerMonitorClient
{
    public sealed class ExternalProgramManager
    {
        private static readonly ExternalProgramManager instance = new ExternalProgramManager();

        private ExternalProgramManager() { }

        public void StartWattTool()
        {
            var p = new Process();
            p.StartInfo.FileName = "apps\\OpenHardwareMonitor\\OpenHardwareMonitor.exe";
            p.StartInfo.Arguments = "";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
        }

        public void StartOpenHardwareMonitor()
        {
            Process[] pname = Process.GetProcessesByName("OpenHardwareMonitor");
            if (pname.Length == 0)
            {
                var p = new Process();
                p.StartInfo.FileName = "apps\\OpenHardwareMonitor\\OpenHardwareMonitor.exe";
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
            }
        }

        public static ExternalProgramManager Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

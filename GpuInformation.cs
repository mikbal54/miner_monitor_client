namespace MinerMonitorClient
{
    partial class Program
    {
        public class GpuInformation
        {
            public string identifier;
            public string fanspeed;
            public string temperature;
            public string load;
            public string coreclock;
            public string memoryclock;

            public GpuInformation()
            {
                identifier = "";
                fanspeed = "";
                temperature = "";
                load = "";
                coreclock = "";
                memoryclock = "";
            }
        };
    }
}

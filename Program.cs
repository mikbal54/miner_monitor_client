using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MinerMonitorClient
{
    partial class Program
    {

        public static string serverAddress = "http://127.0.0.1:8080";
        public static dynamic config;
        static public string localip;


        public static string SHA512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }


        public static  void SendInformation()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                //     ManagementObjectSearcher mos = new ManagementObjectSearcher(@"root\OpenHardwareMonitor", "select * From Sensor where name like \" %GPU% \" and SensorType=\"temperature\"");
                ManagementScope Scope;
                Scope = new ManagementScope(String.Format("\\\\{0}\\root\\OpenHardwareMonitor", "."), null);

                bool hasOpenHardware = false;

                Dictionary<String, GpuInformation> gpuInfo = new Dictionary<string, GpuInformation>();

                try
                {
                    Scope.Connect();
                    hasOpenHardware = true;
                }
                catch (Exception e)
                {

                }

                if(hasOpenHardware)
                {
                    ObjectQuery Query = new ObjectQuery("SELECT * FROM Sensor Where Name LIKE '%GPU%'");
                    ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);



                    //Get All Gpu information
                    foreach (ManagementObject WmiObject in Searcher.Get())
                    {


                        if (WmiObject["SensorType"].ToString() == "Temperature")
                        {
                            string gpuId = WmiObject["Identifier"].ToString().Split('/')[2];
                            GpuInformation gpu;
                            gpuInfo.TryGetValue(gpuId, out gpu);
                            if (gpu == null)
                            {
                                gpu = new GpuInformation();
                                gpu.identifier = gpuId;
                            }

                            gpu.temperature = WmiObject["Value"].ToString();

                            gpuInfo[gpuId] = gpu;
                        }
                        else if (WmiObject["SensorType"].ToString() == "Load")
                        {
                            string gpuId = WmiObject["Identifier"].ToString().Split('/')[2];
                            GpuInformation gpu;
                            gpuInfo.TryGetValue(gpuId, out gpu);
                            if (gpu == null)
                            {
                                gpu = new GpuInformation();
                                gpu.identifier = gpuId;
                            }

                            gpu.load = WmiObject["Value"].ToString();

                            gpuInfo[gpuId] = gpu;
                        }
                        else if (WmiObject["SensorType"].ToString() == "Fan")
                        {
                            string gpuId = WmiObject["Identifier"].ToString().Split('/')[2];
                            GpuInformation gpu;
                            gpuInfo.TryGetValue(gpuId, out gpu);
                            if (gpu == null)
                            {
                                gpu = new GpuInformation();
                                gpu.identifier = gpuId;
                            }

                            gpu.fanspeed = WmiObject["Value"].ToString();

                            gpuInfo[gpuId] = gpu;
                        }
                        else if (WmiObject["SensorType"].ToString() == "Clock")
                        {
                            if (WmiObject["Name"].ToString() == "GPU Memory")
                            {
                                string gpuId = WmiObject["Identifier"].ToString().Split('/')[2];
                                GpuInformation gpu;
                                gpuInfo.TryGetValue(gpuId, out gpu);
                                if (gpu == null)
                                {
                                    gpu = new GpuInformation();
                                    gpu.identifier = gpuId;
                                }

                                gpu.memoryclock = WmiObject["Value"].ToString();

                                gpuInfo[gpuId] = gpu;
                            }
                            else if (WmiObject["Name"].ToString() == "GPU Core")
                            {
                                string gpuId = WmiObject["Identifier"].ToString().Split('/')[2];
                                GpuInformation gpu;
                                gpuInfo.TryGetValue(gpuId, out gpu);
                                if (gpu == null)
                                {
                                    gpu = new GpuInformation();
                                    gpu.identifier = gpuId;
                                }

                                gpu.coreclock = WmiObject["Value"].ToString();

                                gpuInfo[gpuId] = gpu;
                            }

                        }


                    }
                }

              

                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
                double now = Math.Floor(diff.TotalSeconds);

                var info = new
                {
                    name = config.name,
                    password = SHA512(config.password.ToString()),
                    localip = GetLocalIPAddress(),
                    gpus = gpuInfo,
                    algo = "ethash", // TODo : get real algo
                    hashrate = MinerProgramsManager.Instance.GetHashRate(0) , // TODO: send real hash rate
                    time = now
                };



                string response = client.UploadString(serverAddress + "/minerInfoUpload", JsonConvert.SerializeObject(info));
            }
        }

        public static string GetLocalIPAddress()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
                return localIP;
            }
        }

        static void Main(string[] args)
        {
            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = r.ReadToEnd();
                config = JsonConvert.DeserializeObject(json);
            }


            GpuSettingsManager.Instance.GetWattToolInis();
            GpuSettingsManager.Instance.ApplyWattToolSettings();

            localip = GetLocalIPAddress();
            serverAddress = config["server"].ToString();

            Console.WriteLine("Sending info to: " + serverAddress);

            ExternalProgramManager.Instance.StartOpenHardwareMonitor();

            try
            {
                MinerProgramsManager.Instance.StartMiners();
            }
            catch(Exception e)
            {
                Console.WriteLine("Unable to start miner, most likely illegal config.json file");
                Console.WriteLine("Exception: " + e.Message);
            }

            while (true)
            {
                try {
                    MinerProgramsManager.Instance.Run();
                    SendInformation();
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }

                // TODO: change this value from config
                Thread.Sleep(3500);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace WindowsServiceZamtest
{
    public class WMIHelper
    {
        public string GetProcessorID()
        {
            string query = "SELECT ProcessorId FROM Win32_Processor";
            using (ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(query))
            {
                ManagementObjectCollection objCollection = objSearcher.Get();
                if (objCollection.Count == 0)
                    return "";
                foreach (ManagementObject obj in objCollection)
                {
                    var a = obj;
                    return obj["ProcessorId"].ToString().Trim();
                }
            }
            return "";
        }

        public string GetHardDriveVolumeSerialNumber()
        {
            string query = "SELECT SerialNumber FROM Win32_DiskDrive";
            using (ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(query))
            {
                ManagementObjectCollection objCollection = objSearcher.Get();
                if (objCollection.Count == 0)
                    return "";
                foreach (ManagementObject obj in objCollection)
                {
                    return obj["SerialNumber"].ToString().Trim();
                }
            }
            return "";
        }

        public string GetOperatingSystem()
        {
            string result = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                result = os["Caption"].ToString();
                break;
            }
            return result;
        }

        public string GetPhysicalMemory()
        {
            double totalMemory = 0;
            string query = "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem";
            using (ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(query))
            {
                ManagementObjectCollection objCollection = objSearcher.Get();
                foreach (ManagementObject obj in objCollection)
                {
                    double dblMemory;
                    if (double.TryParse(Convert.ToString(obj["TotalPhysicalMemory"]), out dblMemory))
                    {
                        totalMemory += (dblMemory / (1024 * 1024 * 1024));
                        //Console.WriteLine("TotalPhysicalMemory is: {0} MB", Convert.ToInt32(dblMemory / (1024 * 1024)));
                        //Console.WriteLine("TotalPhysicalMemory is: {0} GB", Convert.ToInt32(dblMemory / (1024 * 1024 * 1024)));
                    }
                }
            }
            return $"{totalMemory.ToString("0.00")} GB";
        }
    }
}

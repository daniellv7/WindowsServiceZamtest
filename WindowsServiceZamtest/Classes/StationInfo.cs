using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceZamtest.Classes
{
    public class StationInfo
    {
        public string UserAccount { get; set; }
        public string UserPassword { get; set; }
        public string Client { get; set; }
        public string ProcessorID { get; set; }
        public string HardDiskSerialNumber { get; set; }
        public string StationKey { get { return ProcessorID + HardDiskSerialNumber; } }
        public string OperatingSystem { get; set; }

    }
}

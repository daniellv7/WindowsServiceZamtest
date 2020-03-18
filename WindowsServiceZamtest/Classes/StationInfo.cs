using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceZamtest.Classes
{
    public class StationInfo
    {
        private string encodedText, plainText;
        public string UserAccount { get; set; }
        public string UserPassword { get; set; }
        public string Client { get; set; }
        public string ProcessorID { get; set; }
        public string HardDiskSerialNumber { get; set; }
        public string StationKey { get { return ProcessorID + HardDiskSerialNumber; }}
        public string OperatingSystem { get; set; }

        internal string EncryptStationKey()
        {
            plainText = ProcessorID + HardDiskSerialNumber;
            var plainTextBytes = Encoding.UTF8.GetBytes(StationKey);
            encodedText = Convert.ToBase64String(plainTextBytes);
            return encodedText;
        }

    }
}

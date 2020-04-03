using System;
using System.Text;

namespace WindowsServiceZamtest.Classes
{
    public class StationInfo
    {
        public string UserAccount { get; set; }
        public string Client { get; set; }
        public string StationKey { get; set; }
        public string OperatingSystem { get; set; }
        public int Sockets { get; set; }

        //internal string EncryptStationKey()
        //{
        //    plainText = ProcessorID + HardDiskSerialNumber;
        //    var plainTextBytes = Encoding.UTF8.GetBytes(StationKey);
        //    encodedText = Convert.ToBase64String(plainTextBytes);
        //    return encodedText;
        //}
    }
}

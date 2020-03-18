using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceZamtest.Classes
{
    public class MQResponse
    {
        public Dictionary<int, string> mqDictionary = new Dictionary<int, string>()
        {
            { 1, "StartTest" },
            { 2, "StopTest" },
            { 3, "StartStepsTransmission" },
            { 4, "StopStepsTransmission" },
            { 5, "Test" }
        };

        public string Command { get; set; }
        public string Response { get; set; }
        public Step Step { get; set; }
        public Error Error { get; set; }
    }
}

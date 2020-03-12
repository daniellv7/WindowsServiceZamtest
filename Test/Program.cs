using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WindowsServiceZamtest;


namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathFile = @"C:\Users\Lear\Desktop\Test.txt";
            string key = "Zamtest1";
            
            ZamtestWS service = new ZamtestWS();
            Crypter c = new Crypter();
            WMIHelper wmi = new WMIHelper();
            //Console.WriteLine(wmi.GetPhysicalMemory());
            service.OnStartTest();
            //service.ConfigWatcher(pathWatcher);
            Console.ReadKey();
            service.OnStopTest();
        }

    }
}

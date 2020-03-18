using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.IO;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using WindowsServiceZamtest.Classes;
using Newtonsoft.Json;
using System.Net.Http;

namespace WindowsServiceZamtest
{
    public partial class ZamtestWS : ServiceBase
    {
        internal FileSystemWatcher watcher;
        internal HubConnection hubConnection;
        internal IHubProxy myHubProxy;
        internal MessageQueue messageReceived, messageSend, typeMQ;
        internal Message msgRec, msgSend;
        public static Int32 j = 0;
        //string watcherPath = @"C:\Leak Tester";
        //string logFilePath = @"C:\Leak Tester\LogFile.txt";
        //string newFilePath = @"C:\Leak Tester\NewFiles";
        internal bool state;
        internal string msgReceiveSignalR, strJson;
        internal string pathJson = @"C:\Leak Tester\stationInfo.json";
        WMIHelper wmi;
        Crypter crypter;
        internal string cryptKey = "Crypter1";
        StationInfo stationInfo;
        string stationInfoJson;
        string loginUrl = "http://softwareserver/Account/Login";
        string hubUrl = "http://softwareserver/signalr/hubs";
        string logoffUrl = "http://softwareserver/Account/Logoff";
        string homeUrl = "http://softwareserver/Home/Index";
        HttpClientHandler handler;
        HttpClient httpClient;
        Error error;
        string json;
        MQResponse mQResponse, mqClass;

        public ZamtestWS()
        {
            InitializeComponent();
        }

        public void OnStartTest()
        {
            //var process = Process.Start(@"C:\Users\Lear\Documents\Visual Studio 2019\Projects\WindowsServiceZamtest\Login\Login\bin\Debug\Login.exe");
            state = true;
            SerializeFile();
            stationInfo = DeserializeFileToClass();
            if (LogUser().Result)
            {
                myHubProxy.Invoke("RegisterStation", DeserializeFileToString());
                ReceiveMQ();
            }
        }

        protected override void OnStart(string[] args)
        {
            OnStartTest();
        }

        public void OnStopTest()
        {
            _ = LogOff();
        }

        protected override void OnStop()
        {
            OnStopTest();
        }

        public  async Task LogOff()
        {
            try
            {
                hubConnection.Stop();
                hubConnection.Dispose();
                httpClient = new HttpClient(handler);
                var response = await httpClient.GetAsync(homeUrl);
                var content = await response.Content.ReadAsStringAsync();
                var requestVerificationToken = ParseRequestVerificationToken(content);
                content = requestVerificationToken;
                response = await httpClient.PostAsync(logoffUrl, new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded"));
                content = await response.Content.ReadAsStringAsync();
                handler.Dispose();
                httpClient.Dispose();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public async Task<bool> LogUser()
        {
            handler = new HttpClientHandler
            {
                CookieContainer = new System.Net.CookieContainer()
            };
            try
            {
                httpClient = new HttpClient(handler);
                var response = await httpClient.GetAsync(loginUrl);
                var content = await response.Content.ReadAsStringAsync();
                var requestVerificationToken = ParseRequestVerificationToken(content);
                content = requestVerificationToken + "&Email=" + stationInfo.UserAccount + "&Password=" + stationInfo.UserPassword + "&RememberMe=False";
                response = await httpClient.PostAsync(loginUrl, new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded"));
                content = await response.Content.ReadAsStringAsync();
                await ConnectHub();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private string ParseRequestVerificationToken(string content)
        {
            var startIndex = content.IndexOf("__RequestVerificationToken");
            if (startIndex == -1)
            {
                return null;
            }
            content = content.Substring(startIndex, content.IndexOf("\" />") - startIndex);
            content = content.Replace("\" type=\"hidden\" value=\"", "=");
            return content;
        }

        public async Task ConnectHub()
        {
            try
            {
                hubConnection = new HubConnection(hubUrl); //("https://developear.net/signalr/hubs"); //("http://localhost:8089/");
                hubConnection.CookieContainer = handler.CookieContainer;
                myHubProxy = hubConnection.CreateHubProxy("MainHub");
                myHubProxy.On<string, string>("fetchCommand", (senderId, command) =>  ParseCommand(senderId, command)) ;
                myHubProxy.On<string, string>("displayMessage", (name, message) => Console.Write("SignalR Message: " + name + ": " + message + "\n"));
                myHubProxy.On<string>("keepAlive", (connectionId) => KeepAliveAck(connectionId));
                await hubConnection.Start();
                //SendMsgToSignalR();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message);
            }
        }

        public void KeepAliveAck(string requester)
        {
            if (requester != null)
            {
                myHubProxy.Invoke("KeepAliveAck", new object[] { requester });
                Console.WriteLine(requester);
            }
        }

        private MessageQueue SetQueue(string name)
        {
            if (MessageQueue.Exists(@".\Private$\" + name))
                return typeMQ = new MessageQueue(@".\Private$\" + name);
            else
                return typeMQ = MessageQueue.Create(@".\Private$\" + name);
        }

        private void ParseCommand(string senderId, string command)
        {
            switch (command)
            {
                case "StartTest":
                    Console.WriteLine("Start recibido de aplicacion Web");
                    EnqueueMessage("StartTest");
                    if (messageSend.Peek() == null)
                    {
                        msgReceiveSignalR = string.Empty;
                        break;
                    }
                    else
                    {
                        error = new Error();
                        error.Description = error.keyValues[1];
                        error.Code = error.keyValues.Keys.ElementAt(0);
                        json = JsonConvert.SerializeObject(error, Formatting.Indented);
                        myHubProxy.Invoke("StartTest", "NOK", json);
                        msgReceiveSignalR = string.Empty;
                        break;
                    }
                case "StopTest":
                    Console.WriteLine("Stop recibido de aplicacion Web");
                    EnqueueMessage("StopTest");
                    msgReceiveSignalR = string.Empty;
                    break;
                case "StartStepsTransmission":
                    Console.WriteLine("StartStepsTransmission recibido de aplicacion Web");
                    EnqueueMessage("StartStepsTransmission");
                    msgReceiveSignalR = string.Empty;
                    break;
                case "StopStepsTransmission":
                    Console.WriteLine("StopStepsTransmission recibido de aplicacion Web");
                    EnqueueMessage("StopStepsTransmission");
                    msgReceiveSignalR = string.Empty;
                    break;
                default:
                    break;
            }
        }

        private void ReceiveMQ()
        {
                Task.Factory.StartNew(() =>
                {
                    messageReceived = SetQueue("SeqToService");
                    while (true)
                    {
                        IAsyncResult ar = messageReceived.BeginPeek(new TimeSpan(10, 0, 0), null, new AsyncCallback(ReceiveCompleted));
                        ar.AsyncWaitHandle.WaitOne();
                    }
                });
        }

        private void ReceiveCompleted(IAsyncResult asyncResult)
        {
            msgRec = messageReceived.EndPeek(asyncResult);
            msgRec = messageReceived.Receive();
            msgRec.Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" });
            //Console.WriteLine($"Mensaje del Secuenciador: {0}", msgRec.Body);
            mQResponse = JsonConvert.DeserializeObject<MQResponse>(msgRec.Body.ToString());
            mqClass = new MQResponse();
            switch (mQResponse.Command)
            {
                case "StartTest":
                    {
                        break;
                    }
                case "StopTest":
                    {
                        break;
                    }
                case "StartStepsTransmission":
                    {
                        json = JsonConvert.SerializeObject(mQResponse.Command, Formatting.Indented);
                        myHubProxy.Invoke("StartStepsTransmissionAck", json);
                        break;
                    }
                case "StopStepsTransmission":
                    {
                        json = JsonConvert.SerializeObject(mQResponse.Command, Formatting.Indented);
                        myHubProxy.Invoke("StopStepsTransmissionAck", json);
                        break;
                    }
                case "Step":
                    {
                        Console.WriteLine("Step Recieve OK");
                        json = JsonConvert.SerializeObject(mQResponse.Step, Formatting.Indented);
                        myHubProxy.Invoke("StepResult", json);
                        break;
                    }
                default:
                    break;
            }
        }

        private void EnqueueMessage(string text)
        {
            messageSend = SetQueue("ServiceToSeq");
            msgSend = new Message();
            msgSend.Body = text;
            msgSend.Label = "Mensaje";
            messageSend.Send(msgSend);
        }

        private void SendMsgToSignalR()
        {
            myHubProxy.Invoke("BroadcastMessage", new object[] { "Hola", DateTime.Now.ToString() });
        }

        private StationInfo DeserializeFileToClass()
        {
            strJson = string.Empty;
            crypter = new Crypter();
            crypter.DecryptFile(pathJson, cryptKey);
            strJson = File.ReadAllText(pathJson);
            StationInfo deserialized = JsonConvert.DeserializeObject<StationInfo>(strJson);
            crypter.EncryptFile(pathJson, cryptKey);
            return deserialized;
        }

        private string DeserializeFileToString()
        {
            strJson = string.Empty;
            crypter = new Crypter();
            crypter.DecryptFile(pathJson, cryptKey);
            stationInfoJson = File.ReadAllText(pathJson);
            crypter.EncryptFile(pathJson, cryptKey);
            return stationInfoJson;
        }

        private bool SerializeFile()
        {
            crypter = new Crypter();
            try
            {
                crypter.DecryptFile(pathJson, cryptKey);
                strJson = JsonConvert.SerializeObject(SetInfo(), Formatting.Indented);
                File.WriteAllText(pathJson, strJson);
                crypter.EncryptFile(pathJson, cryptKey);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private StationInfo SetInfo()
        {
            wmi = new WMIHelper();
            StationInfo stationInfo = new StationInfo
            {
                Client = "Lear",
                UserAccount = "daniel.larios@zamtest.com",
                UserPassword = "!Q2w3e4r",
                HardDiskSerialNumber = wmi.GetHardDriveVolumeSerialNumber(),
                ProcessorID = wmi.GetProcessorID(),
                OperatingSystem = wmi.GetOperatingSystem()
            };
            return stationInfo;
        }


        #region File System Watcher
        private void ConfigWatcher(string pathWatcher)
        {
            watcher = new FileSystemWatcher(pathWatcher);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.Renamed += Watcher_Renamed;
            watcher.Deleted += Watcher_Deleted;
            watcher.Created += Watcher_Created;
            watcher.Changed += Watcher_Changed;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {

        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {

        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            
        }
        #endregion

    }
}

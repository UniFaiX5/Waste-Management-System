using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RtspClientSharpCore;
using FrameDecoderCore;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using RtspClientSharpCore.RawFrames;
using System.Diagnostics;
using System.Printing;
using System.Windows.Interop;
using Microsoft.Data.SqlClient;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net;
using System.Security.Cryptography;
using System.IO.Ports;

using System.Data.Common;
using System.Collections;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Azure;
using Newtonsoft.Json.Linq;
using System.IO.Ports;
using System.Net.Sockets;
using SimpleTCP;
using System.Windows;

namespace TestRtspClient
{
    public partial class WasteManagementSystem : Form

    {

       
        
        private const string urlToCamera = "rtsp://rtsp.stream/pattern";
        private const string pathToSaveImage = @"C:\ImageMLProjects\RTSPTrials\RTSPClientNetCore\";
        private const int streamWidth = 320;//240;
        private const int streamHeight = 240;//160;

        static bool SaveImages = true;

        
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static int _imageNumber = 1;
        private static readonly FrameDecoder FrameDecoder = new FrameDecoder();
        private static readonly FrameTransformer FrameTransformer = new FrameTransformer(streamWidth, streamHeight);
        public string location;
        public string material;
        public string weight ;
      //  string weightstr;
        int weightint;
        public string imageloaction;

      

        List<ScaleClass> ScaleList = new List<ScaleClass>();
        public ScaleClass Waste = new ScaleClass();
        string SelectedScale = "";
        string str;
       


        public WasteManagementSystem()
        {
         InitializeComponent();



           

        }


        public class ScaleClass
        {

            public string ScaleSerial { get; set; }
            public Socket Socket { get; set; }
            public NetworkStream ns { get; set; }
            public bool IsRealScale { get; set; }
            public string IP { get; set; }
            public string ScaleResultString { get; set; }
            public string scaleUOM { get; set; }
            public string tString { get; set; }
            public double ScaleReading { get; set; }
            public double TareWeightReading { get; set; }
            public string ScaleName { get; set; }
            public int ScalePort { get; set; }
            public int ConnectionCounter { get; set; }
            public bool ScaleRequired { get; set; }
            public bool Connected { get; set; }


        }



        public void SerialPortProgram()
        {

          
            Waste.ScaleName = "Waste";
            Waste.ScaleSerial = "B848846678";
            Waste.IsRealScale = false;
            Waste.IP = "10.10.35.53";
            Waste.ScalePort = 10001;
            Waste.ConnectionCounter = 0;
            Waste.ScaleRequired = false;
            Waste.Connected = false;
            Waste.Socket = null;
            Waste.ns = null;
           
            
                string tStrings = "";
                
                    try
                    {
                        IPEndPoint printerIPF = null;
                        string printerIP = Waste.IP;
                       if (printerIP != null)
                        {
                           printerIPF = new IPEndPoint(IPAddress.Parse(printerIP), Waste.ScalePort);
                       }
                          Waste.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                          Waste.Socket.Connect(printerIPF);
                          Waste.ns = new NetworkStream(Waste.Socket);
                 
                          Waste.Connected = Waste.Socket.Connected;

                          Waste.Socket.SendTimeout = 1000;
                          Waste.Socket.ReceiveTimeout = 3000;
                  
                           byte[] toSend = Encoding.ASCII.GetBytes("P" + (char)0x0D + (char)0x0A); //Command Weighing Scale in ASCII 7                    
                          Waste.ns.Write(toSend, 0, toSend.Length);

                           byte[] buffer = new byte[1024];
                          Thread.Sleep(200);


             try
            {
           
                     int bytesRead = Waste.Socket.Receive(buffer);
                   Thread.Sleep(1000);
                   tStrings = Encoding.ASCII.GetString(buffer, 0, bytesRead); //string received from scale 4_A_“123456”
                   Thread.Sleep(1000);
                   string[] results = tStrings.Split('"'); // detecting ""

                          weight = results[0];
                           // results = null;
                          str = weight.Substring(0, 14);
                       //   weightstr = str;
                         

                           try { Waste.Socket.Shutdown(SocketShutdown.Both); } catch { }
                          try { Waste.Socket.Close(); } catch { }
                          Thread.Sleep(1000);
                       
                        }         catch { }
                  
            
                    }


                    catch (SocketException ex)
                    {
                     Waste.Connected = false;
                     Waste.ConnectionCounter = 0;
                     Waste.ns = null;
                        try { Waste.Socket.Shutdown(SocketShutdown.Both); } catch { }
                        try { Waste.Socket.Close(); } catch { }
                        Thread.Sleep(1000);
                        // AddStatus("Could not connet to " + Scale.ScaleName + ex.Message, "Error");
                    }
               
                









            
        }
      






        private void Form1_Load(object sender, EventArgs e)
        {
           
           // Thread trdidsp = new Thread(initialdisplayrunner);

           // trdidsp.Start();


            initialdisplayrunner();

           // SerialPortProgram();

            // UIupdate();


            //System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            //timer1.Interval = 300000;//5 minutes
            //timer1.Tick += new System.EventHandler(timer1_Tick);
            //timer1.Start();








        }
        




        private static async Task SaveOnePicture(ConnectionParameters connectionParameters)
        {
            Bitmap bitmap = null;
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                using var rtspClient = new RtspClient(connectionParameters);
                rtspClient.FrameReceived += delegate (object o, RawFrame rawFrame)
                {
                    if (!(rawFrame is RtspClientSharpCore.RawFrames.Video.RawVideoFrame rawVideoFrame))
                        return;

                    var decodedFrame = FrameDecoder.TryDecode(rawVideoFrame);

                    if (decodedFrame == null) return;

                    bitmap = FrameTransformer.TransformToBitmap(decodedFrame);
                    cancellationTokenSource.Cancel();
                };

             
                await rtspClient.ConnectAsync(cancellationTokenSource.Token);
            
                await rtspClient.ReceiveAsync(cancellationTokenSource.Token);

            }
            catch (OperationCanceledException)
            {
            }
          
           // bitmap?.Save(Path.Combine(pathToSaveImage, "image.jpg"), ImageFormat.Jpeg);
        }

        private static void SaveManyPicture(ConnectionParameters connectionParameters)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var connectTask = ConnectAsync(connectionParameters, cancellationTokenSource.Token);

            

            cancellationTokenSource.Cancel();

           
        }
        private static async Task ConnectAsync(ConnectionParameters connectionParameters, CancellationToken token)
        {
            try
            {
                var delay = TimeSpan.FromSeconds(5);

                using (var rtspClient = new RtspClient(connectionParameters))
                {
                    rtspClient.FrameReceived += RtspClient_FrameReceived;

                    while (true)
                    {
                        try
                        {
                           
                            await rtspClient.ConnectAsync(token);
                          
                            await rtspClient.ReceiveAsync(token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                        catch (RtspClientSharpCore.Rtsp.RtspClientException e)
                        {
                            Console.WriteLine(e.ToString());
                            await Task.Delay(delay, token);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static void RtspClient_FrameReceived(object sender, RtspClientSharpCore.RawFrames.RawFrame rawFrame)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (!(rawFrame is RtspClientSharpCore.RawFrames.Video.RawVideoFrame rawVideoFrame))
                return;

            var decodedFrame = FrameDecoder.TryDecode(rawVideoFrame);

            if (decodedFrame == null)
                return;

            var bitmap = FrameTransformer.TransformToBitmap(decodedFrame);

            var fileName = $"image{_imageNumber++}.jpg";

            var frameType = rawFrame is RtspClientSharpCore.RawFrames.Video.RawH264IFrame ? "IFrame" : "PFrame";
            sw.Stop();
            
          
        }





        private void button1_Click(object sender, EventArgs e)
        {
                 //save
            var serverUri = new Uri(urlToCamera);
            //var credentials = new NetworkCredential("admin", "admin12345678");
            var connectionParameters = new ConnectionParameters(serverUri/*, credentials*/);
            //SaveManyPicture(connectionParameters);
            SaveOnePicture(connectionParameters).Wait();

          

        }

        public void AppendpannelPM(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendpannelPM), new object[] { value });
                return;
            }
          //  button17.Visible = true;
            label12.Visible = true;
            //Items.Visible = false;
            

        }
        public void Appendpannelwarehouse(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendpannelwarehouse), new object[] { value });
                return;
            }
           // button17.Visible = true;
           // Items.Visible = true;
            label12.Visible = true;
            

        }

        public void AppendpannelHome(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendpannelHome), new object[] { value });
                return;
            }
            Home.Visible = true;
            
            label14.Visible = true;
            button3.Visible = false;
            MAKING.Visible = false;
            STAGGING.Visible = false;
            WRAEHOUSE.Visible = false;
            WORKSHOP.Visible = false;
            button17.Visible = false;   

            button4.Visible = true;
            button5.Visible = true;
            button6.Visible = true;
            button7.Visible = true;
            button2.Visible = true;
            label1.Visible = true;

        }
        public void Appendpannelclear(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendpannelclear), new object[] { value });
                return;
            }
            
            MAKING.Visible = false;
            STAGGING.Visible = false;
            WRAEHOUSE.Visible = false;
            WORKSHOP.Visible = false;
            
        }

        public void Appendbutton1vt(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendbutton1vt), new object[] { value });
                return;
            }
            button1.Visible = true;
        }

        public void Appendbutton1vf(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendbutton1vf), new object[] { value });
                return;
            }
            button1.Visible = false;
        }
        public void Appendhistvf(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendhistvf), new object[] { value });
                return;
            }
            History.Visible = false;
        }

        public void Appendpannelworkshop(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendpannelworkshop), new object[] { value });
                return;
            }
           // button17.Visible = true;
           // Items.Visible = false;
            label12.Visible = true;
            //SerialPortProgram();
           // textBox1.Text = str;

        }


        public void Appendpannelstagging(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendpannelstagging), new object[] { value });
                return;
            }
           // button17.Visible = true;
            //label12.Visible = true;
            //Items.Visible = false;
            //SerialPortProgram();
            //textBox1.Text = str;
        }

        public void Appendselectlocation(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendselectlocation), new object[] { value });
                return;
            }
            button14.Visible = true;
           

        }

        public void Appendweight(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendweight), new object[] { value });
                return;
            }
            textBox1.Text = str;


        }

        public void AppendweightL(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendweightL), new object[] { value });
                return;
            }
            textBox1.Text = "--------------";


        }



        public void initialdisplayrunner()
        {
            Appendpannelclear("0");
            AppendpannelHome("0");
            Appendbutton1vf("0");
            Appendhistvf("0");
            AppendweightL("0");
           

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

      

        private void label6_Click(object sender, EventArgs e)
        {

        }

      

        private void label10_Click(object sender, EventArgs e)
        {

        }

      

        private void button17_Click(object sender, EventArgs e)
        {
            initialdisplayrunner();

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

      

        private static string nonQueryExecuter(string queryString)   //for SQL queries that would not yeild a 
        {

            string WMSSiterURL = "https://spcsite1p.azurewebsites.net/QueryRecieverPage";

            // string WMSSiterURL = "https://wmssite1p.azurewebsites.net/QueryManager";

            string response = PostinNewThread(WMSSiterURL, queryString);


            return response;
        }

        public static string PostinNewThread(string SiteURL, string prebuiltCommandString)
        {
            string response = "";
            bool SuccessfullUpload = true;
            response = SendPost(SiteURL, prebuiltCommandString);


            if (response.Contains("Success"))
            {
                SuccessfullUpload = true;
            }
            else
            {

                SuccessfullUpload = false;
            }
            try
            {
                string[] stringbreakup = response.Split(new string[] { "?????" }, StringSplitOptions.None);
                if (stringbreakup.Count() >= 2)
                {
                    response = DecrypterRCA(stringbreakup[1]);
                }
            }
            catch (Exception ex)
            {
                //AddStatus("Unable to process reply from Web service " + ex.Message.ToString(), "Error");
            }

            return response;

        }
        public static string SendPost(string url, string postData)  //encrypts and posts data to web api
        {
            string webpageContent = string.Empty;
            //postData = "hello();,.";
            postData = EncrypterRCA(postData);  //built in encryption
                                                //"TYe/IWFI4VOkio4i8VzbBF+KTKC3/L3CNBdy5bM2SRp+FM2v5QrIYPmjWZCSbsjTjgpKobpuCGckunLPz8p46ZZwOwVbQNDlHTt2gDsm0AH43tX0FCubHMfhROOLlHPDtSNOaam6Yohq1pMjtpqTILBeU6XUw7+DdTbBmUxkj3eMbuPsU2j5jTK1F/TNQdgkdfNivRzFuqSXR9suvRiF0V4a8DO+rgUs"
                                                //"TYe/IWFI4VOkio4i8VzbBF KTKC3/L3CNBdy5bM2SRp FM2v5QrIYPmjWZCSbsjTjgpKobpuCGckunLPz8p46ZZwOwVbQNDlHTt2gDsm0AH43tX0FCubHMfhROOLlHPDtSNOaam6Yohq1pMjtpqTILBeU6XUw7 DdTbBmUxkj3eMbuPsU2j5jTK1F/TNQdgkdfNivRzFuqSXR9suvRiF0V4a8DO rgUs"

            //postData = Decrypt(postData, IVStr, KeyStr);
            postData = postData.Replace('+', ';');
            postData = "MyVar1=0&MyVar2=0&Time=" + DateTime.UtcNow.Ticks.ToString() + "&CompiledQueryString=" + postData;
            //adding time value to string to ensure that it always stays unique

            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;
                webRequest.Proxy = null;
                //webRequest.Proxy =WebProxy.GetDefaultProxy();

                webRequest.Credentials = GetCredential();// CredentialCache.DefaultCredentials;
                webRequest.Timeout = 30000;
                webRequest.ReadWriteTimeout = 10000;

                //For Basic Authentication
                //string authInfo = "dmzapiuser" + ":" + "Unilever.123";
                //authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));


                //webRequest.Accept = "application/json; charset=utf-8";

                //webRequest.Headers["Authorization"] = "Basic " + authInfo;

                using (System.IO.Stream webpageStream = webRequest.GetRequestStream())
                {
                    webpageStream.Write(byteArray, 0, byteArray.Length);
                    webpageStream.Flush();
                    webpageStream.Close();
                }

                //HttpWebRequest webResponse = (HttpWebRequest)WebRequest.Create("url");

                //webResponse.Timeout = 10000;

                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {

                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        webpageContent = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                webpageContent = ex.ToString();//throw or return an appropriate response/exception
            }

            return webpageContent;
        }

        private static CredentialCache GetCredential()
        {
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            CredentialCache credentialCache = new CredentialCache();
            credentialCache.Add(new System.Uri("http://172.19.26.3/dmzapi/"), "Basic", new NetworkCredential("dmzapiuser", "Unilever.123"));
            return credentialCache;
        }



        protected static string KeyStrRCA = "K5WMN40BvmnxxjIrYfuGnQ==";
        protected static string IVStrRCA = "SSGYdGTScr0=";

      
        public static string EncrypterRCA(string Message)
        {
            // Create a new instance of the RC2CryptoServiceProvider class
            // and automatically generate a Key and IV.
            RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider();
            rc2CSP.Padding = PaddingMode.PKCS7;
            rc2CSP.KeySize = 128;
            rc2CSP.BlockSize = 64;

            rc2CSP.Mode = CipherMode.CBC;

            // Get the key and IV.

            byte[] key = Convert.FromBase64String(KeyStrRCA);
            byte[] IV = Convert.FromBase64String(IVStrRCA);



            // Get an encryptor.
            ICryptoTransform encryptor = rc2CSP.CreateEncryptor(key, IV);

            // Encrypt the data as an array of encrypted bytes in memory.
            MemoryStream msEncrypt = new MemoryStream();
            CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

            // Convert the data to a byte array.
            string original = Message;
            byte[] toEncrypt = Encoding.ASCII.GetBytes(original);

            // Write all data to the crypto stream and flush it.
            csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
            csEncrypt.FlushFinalBlock();


            // Get the encrypted array of bytes.
            byte[] encrypted = msEncrypt.ToArray();

            string EnrcypStr = Convert.ToBase64String(encrypted);

            //appending identification characters to string
            //EnrcypStr = ";;;" + EnrcypStr + ";;;";
            ///////////////////////////////////////////////////////
            // This is where the data could be transmitted or saved.          
            ///////////////////////////////////////////////////////


            return EnrcypStr;
            // Display the original data and the decrypted data.

        }
        public static string DecrypterRCA(string EnrcypStr)
        {

            byte[] encrypted = Convert.FromBase64String(EnrcypStr); // encryptedT; // Convert.ToBase64String(YourByteArray)

            byte[] key = Convert.FromBase64String(KeyStrRCA);
            byte[] IV = Convert.FromBase64String(IVStrRCA);
            RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider();


            rc2CSP.Padding = PaddingMode.PKCS7;
            rc2CSP.KeySize = 128;
            rc2CSP.BlockSize = 64;

            rc2CSP.Mode = CipherMode.CBC;
            //Get a decryptor that uses the same key and IV as the encryptor.
            ICryptoTransform decryptor = rc2CSP.CreateDecryptor(key, IV);

            // Now decrypt the previously encrypted message using the decryptor
            // obtained in the above step.
            MemoryStream msDecrypt = new MemoryStream(encrypted);
            CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

            // Read the decrypted bytes from the decrypting stream
            // and place them in a StringBuilder class.

            StringBuilder roundtrip = new StringBuilder();

            int b = 0;
            try
            {
                do
                {
                    b = csDecrypt.ReadByte();

                    if (b != -1)
                    {
                        roundtrip.Append((char)b);
                    }

                } while (b != -1);
            }
            catch (Exception ex)
            {
                roundtrip.Clear();
            }


            return roundtrip.ToString();
        }


  

        private void button1_Click_1(object sender, EventArgs e)
            
        {



            //  var serverUri = new Uri(urlToCamera);
            // var connectionParameters = new ConnectionParameters(serverUri/*, credentials*/);
            // SaveOnePicture(connectionParameters).Wait();

            SerialPortProgram();
            //weightint =  Convert.ToInt32(weightstr);
            Appendweight("0");


          

            if (location == "nill" )
            { label14.Visible = true;                     }
            else
            {

                string Timeticks;
                Timeticks = DateTime.Now.Ticks.ToString();

                string prebuiltCommandString = null;


                prebuiltCommandString = "INSERT INTO WasteManagementSystem ([KEY],[Location],[Material],[Weight],[Datetime]) VALUES ('" + Timeticks + "','" + location.ToString() + "','" + material.ToString() + "','" + weight.ToString() + "','" + DateTime.Now.ToString("MM-dd-yyyy h:mm tt") + "')";
                nonQueryExecuter(prebuiltCommandString);



                initialdisplayrunner(); 
            }
          


            location = "nill";
            material = "nill";
        
        }



        private void button25_Click_1(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Cardboard/Paper";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;

        }

        private void button24_Click_2(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Clean Plastic";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button23_Click_2(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Plastic Bottle";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button22_Click_2(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Strip";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button21_Click_2(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Used PPE";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button20_Click_1(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Tissue";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button33_Click_1(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "General Waste";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button32_Click_2(object sender, EventArgs e)
        {
            location = "Stagging";
            material = "Cardboard/Paper";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button34_Click_1(object sender, EventArgs e)
        {
            location = "Stagging";
            material = "Clean Plastic";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button19_Click_3(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Cardboard/Paper";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button31_Click_2(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Clean Plastic";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button30_Click_1(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Dirty Cotton";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button29_Click_1(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Cartridge";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;

        }

        private void button28_Click_2(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Tissue";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button27_Click_1(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Used PPE";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button26_Click_1(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Metal Scrap";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button18_Click_1(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "General Waste";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button8_Click_3(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Used Packing Material";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button9_Click_2(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Used bags";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button10_Click_2(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Paper Rolls";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button13_Click_2(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Plastic Bottle";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Foam";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button11_Click_1(object sender, EventArgs e)
        {

            location = "Packing and Making";
            material = "Foil";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button16_Click_1(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Filled IBC";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button15_Click_1(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Used PPE";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button14_Click_1(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "General Waste";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

  

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button2.Visible = false;
            label1.Visible = false;


            SerialPortProgram();
           // weightint = Convert.ToInt32(weightstr);

            textBox1.Text = str;

            label14.Visible = false;
            MAKING.Visible = true;
            STAGGING.Visible = false;
            WRAEHOUSE.Visible = false;
            WORKSHOP.Visible = false;

            label4.Visible = true;
            label9.Visible = false;
            WAREHOUSE.Visible = false;

            label15.Visible = false;

            label12.Visible = true;

            button8.Visible = true;
            button9.Visible = true;
            button10.Visible = true;
            button13.Visible = true;
            button12.Visible = true;
            button11.Visible = true;
            button16.Visible = true;
            button15.Visible = true;
            button14.Visible = true;

            /*
            button32.Visible = false;
            button34.Visible = false;
            button23.Visible = false;
            button33.Visible = false;
            button25.Visible = false;
            button24.Visible = false;
            button22.Visible = false;
            button21.Visible = false;
            button20.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button31.Visible = false;
            button27.Visible = false;
            button28.Visible = false;
            button30.Visible = false;
            button29.Visible = false;
            button26.Visible = false;
            */
            button17.Visible = true;

           


        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button2.Visible = false;
            label1.Visible = false;



            SerialPortProgram();
           // weightint = Convert.ToInt32(weightstr);
            textBox1.Text = str;

            label14.Visible = false;
           

            label4.Visible = false;
            label9.Visible = false;
            WAREHOUSE.Visible = false;
            label15.Visible = true;
            MAKING.Visible = false;
            STAGGING.Visible = false;
            WRAEHOUSE.Visible = false;
            WORKSHOP.Visible = true;

            label12.Visible = true;
            /*
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button13.Visible = false;
            button12.Visible = false;
            button11.Visible = false;
            button16.Visible = false;
            button15.Visible = false;
            button14.Visible = false;
            button32.Visible = false;
            button34.Visible = false;
            button23.Visible = false;
            button33.Visible = false;
            button25.Visible = false;
            button24.Visible = false;
            button22.Visible = false;
            button21.Visible = false;
            button20.Visible = false;
           */
            button18.Visible = true;
            button19.Visible = true;
            button31.Visible = true;
            button27.Visible = true;
            button28.Visible = true;
            button30.Visible = true;
            button29.Visible = true;
            button26.Visible = true;


            button17.Visible = true;
           


        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button2.Visible = false;
            label1.Visible = false;



            SerialPortProgram();

           // weightint = Convert.ToInt32(weightstr);
            textBox1.Text = str;


            label14.Visible = false;
           

            label4.Visible = false;
            label9.Visible = false;
            WAREHOUSE.Visible = true;
            MAKING.Visible = false;
            STAGGING.Visible = false;
            WRAEHOUSE.Visible = true;
            WORKSHOP.Visible = false;
            label15.Visible = false;
           
            label12.Visible = true;

            button23.Visible = true;
            button33.Visible = true;
            button25.Visible = true;
            button24.Visible = true;
            button22.Visible = true;
            button21.Visible = true;
            button20.Visible = true;
            /*
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button13.Visible = false;
            button12.Visible = false;
            button11.Visible = false;
            button16.Visible = false;
            button15.Visible = false;
            button14.Visible = false;
            button32.Visible = false;
            button34.Visible = false;
           
            button18.Visible = false;
            button19.Visible = false;
            button31.Visible = false;
            button27.Visible = false;
            button28.Visible = false;
            button30.Visible = false;
            button29.Visible = false;
            button26.Visible = false;
            */

            button17.Visible = true;

          
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button2.Visible = false;
            label1.Visible = false;




            SerialPortProgram();
           // weightint = Convert.ToInt32(weightstr);
            textBox1.Text = str;


            label14.Visible = false;
            MAKING.Visible = false;
            STAGGING.Visible = true;
            WRAEHOUSE.Visible = false;
            WORKSHOP.Visible = false;

            label4.Visible = false;
            label9.Visible = true;
            WAREHOUSE.Visible = false;
            label15.Visible = false;

            label12.Visible = true;

            button32.Visible = true;
            button34.Visible = true;

            /*
            button8.Visible = false;
            button9.Visible = false;
            button10.Visible = false;
            button13.Visible = false;
            button12.Visible = false;
            button11.Visible = false;
            button16.Visible = false;
            button15.Visible = false;
            button14.Visible = false;
          
            button23.Visible = false;
            button33.Visible = false;
            button25.Visible = false;
            button24.Visible = false;
            button22.Visible = false;
            button21.Visible = false;
            button20.Visible = false;
            button18.Visible = false;
            button19.Visible = false;
            button31.Visible = false;
            button27.Visible = false;
            button28.Visible = false;
            button30.Visible = false;
            button29.Visible = false;
            button26.Visible = false;
            */

            button17.Visible = true;

        
           


        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Home.Visible = false;
            button3.Visible = true;
            History.Visible = true;
            dataGridView1.Rows.Clear();



          
            DataTable ReturnedDataTable = new DataTable();
            DateTime Query = DateTime.Now.AddDays(0);
            string Querystring = "select * FROM [dbo].[WasteManagementSystem] where [datetime] >='" + Query.ToString("yyyy-MM-dd") + "'order by [datetime] desc";
            ReturnedDataTable = QueryExecuterQE(Querystring);

           


            foreach (DataRow row in ReturnedDataTable.Rows)
            {

                dataGridView1.Rows.Add(row["DateTime"].ToString(), row["Location"].ToString(), row["Material"].ToString(), row["Weight"].ToString());


            }


           
        }


        private DataTable QueryExecuterQE(string queryString)
        {
            DataTable ReturnedDataTable = new DataTable();
            string WMSSiterURL = "https://spcsite1p.azurewebsites.net/QueryExecuter"; // inside spc
                                                                                      // string WMSSiterURL = "https://wmssite1p.azurewebsites.net/QueryManager";
            string response = PostinNewThreadQE(WMSSiterURL, queryString);

            try
            {
                ReturnedDataTable = (DataTable)JsonConvert.DeserializeObject(response, (typeof(DataTable)));
            }
            catch
            {

            }
            return ReturnedDataTable;
        }
        public string PostinNewThreadQE(string SiteURL, string prebuiltCommandString)
        {
            string response = "";
            bool SuccessfullUpload = true;
            response = SendPost(SiteURL, prebuiltCommandString);


            if (response.Contains("Success"))
            {
                SuccessfullUpload = true;
            }
            else
            {

                SuccessfullUpload = false;
            }
            try
            {
                string[] stringbreakup = response.Split(new string[] { "?????" }, StringSplitOptions.None);
                if (stringbreakup.Count() >= 2)
                {
                    response = DecrypterRCA(stringbreakup[1]);
                }
            }
            catch (Exception ex)
            {

            }





            return response;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            initialdisplayrunner();
        }

        private void Items_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button35_Click(object sender, EventArgs e)
        {
            location = "Stagging";
            material = "Strip";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }

        private void button36_Click(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Cap B Bag";
            button1.Visible = true;
            label14.Visible = false;
            label12.Visible = false;
        }
    }



 }







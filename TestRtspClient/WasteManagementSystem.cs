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


//using System.Data.Sql;
using System.Data.Common;
using System.Collections;
using Microsoft.VisualBasic;

namespace TestRtspClient
{
    public partial class Form1 : Form

    {

       
        // TODO: Change to your values
        private const string urlToCamera = "rtsp://rtsp.stream/pattern";
        //    private const string urlToCamera = "rtsp://MirrorBoy:123454321@192.168.0.189/h264Preview_01_main";
        private const string pathToSaveImage = @"C:\ImageMLProjects\RTSPTrials\RTSPClientNetCore\";
        private const int streamWidth = 320;//240;
        private const int streamHeight = 240;//160;

        static bool SaveImages = true;

        
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
       // private static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private static int _imageNumber = 1;
        private static readonly FrameDecoder FrameDecoder = new FrameDecoder();
        private static readonly FrameTransformer FrameTransformer = new FrameTransformer(streamWidth, streamHeight);
        public string location;
        public string material;
        public string weight = "0";
        public string imageloaction;

        SqlConnection con;
        SqlCommand cmd;
        DataTable History = new DataTable();


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            Thread trdidsp = new Thread(initialdisplayrunner);

            trdidsp.Start();

         



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
            packingmaking.Visible = true;
            warehouse.Visible = false;
            workshop.Visible = false;
            Packstagging.Visible = false;   

        }
        public void Appendpannelwarehouse(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendpannelwarehouse), new object[] { value });
                return;
            }
            warehouse.Visible = true;
            workshop.Visible = false;
            Packstagging.Visible = false;
            packingmaking.Visible = false;
        }
        public void Appendpannelclear(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendpannelclear), new object[] { value });
                return;
            }
            warehouse.Visible = false;
            workshop.Visible = false;
            Packstagging.Visible = false;
            packingmaking.Visible = false;
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
            panel2.Visible = false;
        }

        public void Appendpannelworkshop(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendpannelworkshop), new object[] { value });
                return;
            }
            warehouse.Visible = false;
            workshop.Visible = true;
            Packstagging.Visible = false;
            packingmaking.Visible = false;
        }


        public void Appendpannelstagging(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Appendpannelstagging), new object[] { value });
                return;
            }
            warehouse.Visible = false;
            workshop.Visible = false;
            Packstagging.Visible = true;
            packingmaking.Visible = false;
        }



        public void initialdisplayrunner()
        {
            Appendpannelclear("0");
            Appendbutton1vf("0");
            Appendhistvf("0");



        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {


           

        }

        private void button5_Click(object sender, EventArgs e)
        {
            packingmaking.Visible = false;
            warehouse.Visible = true;
            workshop.Visible = false;
            Packstagging.Visible = false;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            packingmaking.Visible = true;
            warehouse.Visible = false;
            workshop.Visible = false;
            Packstagging.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            packingmaking.Visible = false;
            warehouse.Visible = false;
            workshop.Visible = true;
            Packstagging.Visible = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            packingmaking.Visible = false;
            warehouse.Visible = false;
            workshop.Visible = false;
            Packstagging.Visible = true;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            initialdisplayrunner();
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
            
        {
           
            

            if (location == "nill" )
            { panel1.Visible = true;                     }
            else
            {        
                con = new SqlConnection(@"Data Source=dpcdatabase1p.database.windows.net;Initial Catalog=dpcdatabase1p;User ID=fahim.rahmathali@unilever.com;Password=Fx5.62451;Authentication=""Active Directory Password""");
                 


                con.Open();
                cmd = new SqlCommand("insert into WasteManagementSystem2 ([Location],[Material],[Weight],[Datetime]) values ('" + location.ToString()  + "','" + material.ToString() + "','" + weight.ToString() + "','" + DateTime.Now + "' ) ",con);
                cmd.ExecuteNonQuery();


              



                initialdisplayrunner(); 
            }
          


            location = "nill";
            material = "nill";
        }

        private void button25_Click(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Cardboard/Paper";
            button1.Visible = true;
            panel1.Visible = false;

        }

        private void button24_Click(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Clean Plastic";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button23_Click(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Plastic Bottle";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Strip";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button21_Click(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Used PPE";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "Tissue";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button33_Click(object sender, EventArgs e)
        {
            location = "Warehouse";
            material = "General Waste";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button32_Click(object sender, EventArgs e)
        {
            location = "Stagging";
            material = "Cardboard/Paper";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button34_Click(object sender, EventArgs e)
        {
            location = "Stagging";
            material = "Clean Plastic";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Cardboard/Paper";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button31_Click(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Clean Plastic";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button30_Click(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Dirty Cotton";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button29_Click(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Cartridge";
            button1.Visible = true;
            panel1.Visible = false;

        }

        private void button28_Click(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Tissue";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button27_Click(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Used PPE";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button26_Click(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "Metal Scrap";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            location = "Workshop";
            material = "General Waste";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Used Packing Material";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Used bags";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Paper Rolls";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Plastic Bottle";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Foam";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button11_Click(object sender, EventArgs e)
        {

            location = "Packing and Making";
            material = "Foil";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Filled IBC";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "Cap B Bag";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            location = "Packing and Making";
            material = "General Waste";
            button1.Visible = true;
            panel1.Visible = false;
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void Packstagging_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel2.Visible = true;
            dataGridView1.Rows.Clear();
            con = new SqlConnection(@"Data Source=dpcdatabase1p.database.windows.net;Initial Catalog=dpcdatabase1p;User ID=fahim.rahmathali@unilever.com;Password=Fx5.62451;Authentication=""Active Directory Password""");



            con.Open();
            cmd = new SqlCommand("select * FROM WasteManagementSystem2 order by [Datetime] desc", con);
            //cmd.ExecuteReader();
              SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                string rw1 = $"Location : {dr.GetValue(0)}, Material:{dr.GetValue(1)}, Weight{dr.GetValue(2)}, Datetime{dr.GetValue(3)} ";

              
                dataGridView1.Rows.Add( dr.GetValue(0), dr.GetValue(1), dr.GetValue(2), dr.GetValue(3));

            }



            //string rw1 = Hist["Material"].ToString();   
            



            con.Close();
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }



 }







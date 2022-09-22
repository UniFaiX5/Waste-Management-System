using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RtspClientSharpCore;
using RtspClientSharpCore.RawFrames;

namespace TestRtspClient
{
    class Program
    {
       


        static void Main()
        {
            Thread trdfrm = new Thread(Formrunner);

            trdfrm.Start();
        }


        private static void Formrunner(object obj)
        {
            Application.Run(new Form1());



            //bool Run = false;
            // while (Run == true)
            //     {



            //    Task.Delay(10000);



            // }







        }






    }
}


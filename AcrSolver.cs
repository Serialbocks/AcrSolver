using System;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AcrSolver
{
    public partial class uxMainWindow : Form
    {
        public uxMainWindow()
        {
            InitializeComponent();
        }

        private void WriteStatusLine(string text)
        {
            uxStatus.AppendText(text + "\r\n");
        }

        private bool isPixelButton(Bitmap screenshot, double xPercentage, double yPercentage)
        {
            var seatX = (int)(xPercentage * screenshot.Width);
            var seatY = (int)(yPercentage * (screenshot.Height - topMargin) + topMargin);
            var seatPixel = screenshot.GetPixel(seatX, seatY);
            return seatPixel.R < 50 && seatPixel.G < 50 && seatPixel.B < 50;
        }

        private int FindButton(Bitmap screenshot)
        {
            if(isPixelButton(screenshot, 0.45824, 0.70448))
            {
                return 1;
            }
            if(isPixelButton(screenshot, 0.28266, 0.60299))
            {
                return 2;
            }
            if (isPixelButton(screenshot, 0.17452, 0.35522))
            {
                return 3;
            }
            if (isPixelButton(screenshot, 0.59422, 0.27612))
            {
                return 4;
            }
            if (isPixelButton(screenshot, 0.76660, 0.28955))
            {
                return 5;
            }
            if (isPixelButton(screenshot, 0.69058, 0.69581))
            {
                return 6;
            }

            return -1;
        }

        const int topMargin = 26;
        private void uxCapture_Click(object sender, EventArgs e)
        {
            var screenshot = Screenshot.PrintWindow();
            if(screenshot == null)
            {
                WriteStatusLine("Could not find game window");
                return;
            }

            // Find the button!
            var buttonSeat = FindButton(screenshot);
            WriteStatusLine(String.Format("Button at seat {0}", buttonSeat));
        }
    }
}

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
            var seat1X = (int)(0.45824 * screenshot.Width);
            var seat1Y = (int)(0.70448 * (screenshot.Height - topMargin) + topMargin);
            var seat1Pixel = screenshot.GetPixel(seat1X, seat1Y);
            WriteStatusLine(String.Format("Seat 1 RGB: {0} {1} {2}", seat1Pixel.R, seat1Pixel.G, seat1Pixel.B));

            var seat2X = (int)(0.28266 * screenshot.Width);
            var seat2Y = (int)(0.60299 * (screenshot.Height - topMargin) + topMargin);
            var seat2Pixel = screenshot.GetPixel(seat2X, seat2Y);
            WriteStatusLine(String.Format("Seat 2 RGB: {0} {1} {2}", seat2Pixel.R, seat2Pixel.G, seat2Pixel.B));

            var seat3X = (int)(0.17452 * screenshot.Width); // 163 264
            var seat3Y = (int)(0.35522 * (screenshot.Height - topMargin) + topMargin);
            var seat3Pixel = screenshot.GetPixel(seat3X, seat3Y);
            WriteStatusLine(String.Format("Seat 3 RGB: {0} {1} {2}", seat3Pixel.R, seat3Pixel.G, seat3Pixel.B));

            var seat4X = (int)(0.59422 * screenshot.Width);
            var seat4Y = (int)(0.27612 * (screenshot.Height - topMargin) + topMargin);
            var seat4Pixel = screenshot.GetPixel(seat4X, seat4Y);
            WriteStatusLine(String.Format("Seat 4 RGB: {0} {1} {2}", seat4Pixel.R, seat4Pixel.G, seat4Pixel.B));

            var seat5X = (int)(0.76660 * screenshot.Width);
            var seat5Y = (int)(0.28955 * (screenshot.Height - topMargin) + topMargin);
            var seat5Pixel = screenshot.GetPixel(seat5X, seat5Y);
            WriteStatusLine(String.Format("Seat 5 RGB: {0} {1} {2}", seat5Pixel.R, seat5Pixel.G, seat5Pixel.B));

            var seat6X = (int)(0.69058 * screenshot.Width);
            var seat6Y = (int)(0.69581 * (screenshot.Height - topMargin) + topMargin);
            var seat6Pixel = screenshot.GetPixel(seat6X, seat6Y);
            WriteStatusLine(String.Format("Seat 6 RGB: {0} {1} {2}", seat6Pixel.R, seat6Pixel.G, seat6Pixel.B));
        }
    }
}

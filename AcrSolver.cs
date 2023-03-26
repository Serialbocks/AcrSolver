using System;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using System.Drawing;

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

        private List<OpenCvSharp.Point> RunTemplateMatch(string reference, string template)
        {
            var matches = new List<OpenCvSharp.Point>();
            using (Mat refMat = new Mat(reference))
            using (Mat tplMat = new Mat(template))
            using (Mat res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1))
            {
                //Convert input images to gray
                Mat gref = refMat.CvtColor(ColorConversionCodes.BGR2GRAY);
                Mat gtpl = tplMat.CvtColor(ColorConversionCodes.BGR2GRAY);

                Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
                Cv2.Threshold(res, res, 0.8, 1.0, ThresholdTypes.Tozero);

                while (true)
                {
                    double minval, maxval, threshold = 0.8;
                    OpenCvSharp.Point minloc, maxloc;
                    Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);

                    if (maxval >= threshold)
                    {
                        //Setup the rectangle to draw
                        var point = new OpenCvSharp.Point(maxloc.X, maxloc.Y);
                        Rect r = new Rect(point, new OpenCvSharp.Size(tplMat.Width, tplMat.Height));
                        matches.Add(point);

                        //Draw a rectangle of the matching area
                        Cv2.Rectangle(refMat, r, Scalar.LimeGreen, 2);

                        //Fill in the res Mat so you don't find the same area again in the MinMaxLoc
                        Rect outRect;
                        Cv2.FloodFill(res, maxloc, new Scalar(0), out outRect, new Scalar(0.1), new Scalar(1.0));
                    }
                    else
                        break;
                }

                return matches;
            }
        }

        private int FindButton(Bitmap screenshot)
        {
            var matches = RunTemplateMatch("test.jpg", "button.jpg");

            if(matches.Count == 0)
            {
                return -1;
            }

            var match = matches[0];
            var third = screenshot.Width / 3;
            if(match.Y > screenshot.Height / 2)
            {
                // Seat 1, 2, or 6
                if(match.X < third)
                {
                    return 2;
                }
                else if(match.X > (2 * third))
                {
                    return 6;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                // seat 3, 4, or 5
                if (match.X < third)
                {
                    return 3;
                }
                else if (match.X > (2 * third))
                {
                    return 5;
                }
                else
                {
                    return 4;
                }
            }
        }

        private void uxCapture_Click(object sender, EventArgs e)
        {
            var screenshot = Screenshot.PrintWindow();
            if(screenshot == null)
            {
                WriteStatusLine("Could not find game window");
                return;
            }

            screenshot.Save("test.jpg", ImageFormat.Jpeg);

            // Find the button!
            var buttonSeat = FindButton(screenshot);
            WriteStatusLine(String.Format("Button at seat {0}", buttonSeat));
        }
    }
}

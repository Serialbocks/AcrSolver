using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace AcrSolver
{
    public static class GameStateDetector
    {
        public static int FindButton(Screenshot screenshot)
        {
            var matches = RunTemplateMatch(screenshot, "button.jpg");
            if (matches.Count == 0)
            {
                return -1;
            }

            return PlayerFromPoint(screenshot, matches[0]);
        }

        public static int FindActivePlayer(Screenshot screenshot)
        {
            var matches = RunTemplateMatch(screenshot, "player-active.jpg");
            if (matches.Count == 0)
            {
                return -1;
            }

            return PlayerFromPoint(screenshot, matches[0]);
        }

        public static List<int> OpponentsWithCards(Screenshot screenshot)
        {
            var result = new List<int>();
            var matches = RunTemplateMatch(screenshot, "opponent-has-cards.jpg");

            foreach(var match in matches)
            {
                var player = PlayerFromPoint(screenshot, match);
                if(player >= 0)
                {
                    result.Add(player);
                }
            }

            result.Sort();
            return result
                .GroupBy(x => x)
                .Select(x => x.First())
                .ToList();
        }

        public static bool PlayerHasCards(Screenshot screenshot)
        {
            var matches = RunTemplateMatch(screenshot, "player-has-cards.jpg");
            return matches.Count > 0;
        }

        private static int PlayerFromPoint(Screenshot screenshot, OpenCvSharp.Point point)
        {
            var third = screenshot.Bitmap.Width / 3;
            if (point.Y > screenshot.Bitmap.Height / 2)
            {
                // Seat 1, 2, or 6
                if (point.X < third)
                {
                    return 2;
                }
                else if (point.X > (2 * third))
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
                if (point.X < third)
                {
                    return 3;
                }
                else if (point.X > (2 * third))
                {
                    return 5;
                }
                else
                {
                    return 4;
                }
            }
        }


        private static List<OpenCvSharp.Point> RunTemplateMatch(Screenshot reference, string template)
        {
            var matches = new List<OpenCvSharp.Point>();
            using (Mat refMat = Mat.FromImageData(reference.Bytes))
            using (Mat tplMat = new Mat(template))
            using (Mat res = new Mat(refMat.Rows - tplMat.Rows + 1, refMat.Cols - tplMat.Cols + 1, MatType.CV_32FC1))
            {
                //Convert input images to gray
                Mat gref = refMat.CvtColor(ColorConversionCodes.BGR2GRAY);
                Mat gtpl = tplMat.CvtColor(ColorConversionCodes.BGR2GRAY);

                double threshold = 0.9;
                Cv2.MatchTemplate(gref, gtpl, res, TemplateMatchModes.CCoeffNormed);
                Cv2.Threshold(res, res, threshold, 1.0, ThresholdTypes.Tozero);

                while (true)
                {
                    double minval, maxval;
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

       
    }
}

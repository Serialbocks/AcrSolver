using System;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AcrSolver
{
    public partial class uxMainWindow : Form
    {
        private OCR _ocr;
        public uxMainWindow()
        {
            InitializeComponent();
            _ocr = new OCR(WriteStatusLine);
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            _ocr.Stop();
        }

        private void WriteStatusLine(string text)
        {
            if(uxStatus.InvokeRequired)
            {
                uxStatus.Invoke(new Action(() => uxStatus.AppendText(text + "\r\n")));
            }
            else
            {
                uxStatus.AppendText(text + "\r\n");
            }
        }

        private string FormatList(List<int> list)
        {
            string result = "";
            foreach(var number in list)
            {
                result += number + ", ";
            }
            if(result.Length > 1)
            {
                result = result.Substring(0, result.Length - 2);
            }
            return result;
        }

        private void uxCapture_Click(object sender, EventArgs e)
        {
            var screenshot = ScreenshotUtils.PrintWindow();
            if(screenshot == null)
            {
                WriteStatusLine("Could not find game window");
                return;
            }

            var filename = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "screenshot.jpg");

            screenshot.Bitmap.Save(filename, ImageFormat.Jpeg);

            var buttonSeat = GameStateDetector.FindButton(screenshot);
            WriteStatusLine(String.Format("Button at seat {0}", buttonSeat));
            
            var activePlayer = GameStateDetector.FindActivePlayer(screenshot);
            WriteStatusLine(String.Format("Active player: {0}", activePlayer));
            
            var opponentsWithCards = GameStateDetector.OpponentsWithCards(screenshot);
            WriteStatusLine(String.Format("Opponents with cards: {0}", FormatList(opponentsWithCards)));
            
            var playerHasCards = GameStateDetector.PlayerHasCards(screenshot);
            WriteStatusLine(String.Format("Player has cards: {0}", playerHasCards));

            _ocr.Process(filename);
        }

        private void uxClear_Click(object sender, EventArgs e)
        {
            uxStatus.Text = String.Empty;
        }
    }
}

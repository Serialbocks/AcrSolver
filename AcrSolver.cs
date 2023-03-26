using System;
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
        public uxMainWindow()
        {
            InitializeComponent();
        }

        private void WriteStatusLine(string text)
        {
            uxStatus.AppendText(text + "\r\n");
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
            var screenshot = Screenshot.PrintWindow();
            if(screenshot == null)
            {
                WriteStatusLine("Could not find game window");
                return;
            }

            screenshot.Save("test.jpg", ImageFormat.Jpeg);

            // Find the button!
            var buttonSeat = GameStateDetector.FindButton(screenshot);
            WriteStatusLine(String.Format("Button at seat {0}", buttonSeat));

            // Find the active player
            var activePlayer = GameStateDetector.FindActivePlayer(screenshot);
            WriteStatusLine(String.Format("Active player: {0}", activePlayer));

            var opponentsWithCards = GameStateDetector.OpponentsWithCards(screenshot);
            WriteStatusLine(String.Format("Opponents with cards: {0}", FormatList(opponentsWithCards)));
        }
    }
}

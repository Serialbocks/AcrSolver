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
        private TexasSolver _texasSolver;
        public uxMainWindow()
        {
            InitializeComponent();
            _ocr = new OCR(WriteStatusLine, OnOcrProcessComplete);
            _texasSolver = new TexasSolver(WriteStatusLine, OnTexasSolveComplete);
            UpdateUX();
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            _ocr.Stop();
            _texasSolver.Stop();
        }

        private void OnTexasSolveComplete()
        {

        }

        private void OnOcrProcessComplete()
        {
            UpdateUX();
        }

        private void UpdateUX()
        {
            // Update pot
            var potVal = GameState.Pot;
            if(potVal > 0)
            {
                SetLabelText(uxPot, "Pot " + GameState.AmountToString(potVal));
            }
            else
            {
                SetLabelText(uxPot, String.Empty);
            }

            // Update board
            SetLabelText(uxBoard, FormatList(GameState.Board));

            foreach (var bet in GameState.Bets)
            {
                WriteStatusLine(String.Format("Position {0} {1} {2}", bet.Position, bet.Type.ToString(), bet.Amount));
            }

            // Update seats
            for (int seatIndex = 0; seatIndex < GameState.Seats.Count; seatIndex++)
            {
                var seat = GameState.Seats[seatIndex];
                Label betLabel = null;
                Label stackLabel = null;
                Label cardsLabel = null;
                switch(seatIndex)
                {
                    case 0:
                        betLabel = uxSeat1Bet;
                        stackLabel = uxSeat1Stack;
                        cardsLabel = uxSeat1Cards;
                        break;
                    case 1:
                        betLabel = uxSeat2Bet;
                        stackLabel = uxSeat2Stack;
                        cardsLabel = uxSeat2Cards;
                        break;
                    case 2:
                        betLabel = uxSeat3Bet;
                        stackLabel = uxSeat3Stack;
                        cardsLabel = uxSeat3Cards;
                        break;
                    case 3:
                        betLabel = uxSeat4Bet;
                        stackLabel = uxSeat4Stack;
                        cardsLabel = uxSeat4Cards;
                        break;
                    case 4:
                        betLabel = uxSeat5Bet;
                        stackLabel = uxSeat5Stack;
                        cardsLabel = uxSeat5Cards;
                        break;
                    case 5:
                        betLabel = uxSeat6Bet;
                        stackLabel = uxSeat6Stack;
                        cardsLabel = uxSeat6Cards;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid seat index!");
                }

                // Update bet size
                if(seat.Bet > 0)
                {
                    SetLabelText(betLabel, GameState.AmountToString(seat.Bet));
                }
                else
                {
                    SetLabelText(betLabel, String.Empty);
                }

                // Update stack size
                if(seat.Stack > 0)
                {
                    SetLabelText(stackLabel, GameState.AmountToString(seat.Stack));
                }
                else
                {
                    SetLabelText(stackLabel, String.Empty);
                }

                // Update cards
                if(!seat.HasCards)
                {
                    SetLabelText(cardsLabel, String.Empty);
                }
                else if(seatIndex == 0)
                {
                    SetLabelText(cardsLabel, FormatList(GameState.PlayerHand));
                }
                else
                {
                    SetLabelText(cardsLabel, "HAS CARDS");
                }

                // Update button
                if(GameState.Button - 1 == seatIndex)
                {
                    AppendLabelText(cardsLabel, " (BUTTON)");
                }
            }
        }

        private void SetLabelText(Label label, string text)
        {
            if(label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.Text = text));
            }
            else
            {
                label.Text = text;
            }
        }

        private void AppendLabelText(Label label, string text)
        {
            if (label.InvokeRequired)
            {
                label.Invoke(new Action(() => label.Text += text));
            }
            else
            {
                label.Text = text;
            }
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
            if (list.Count == 0)
                return result;
            foreach (var number in list)
            {
                result += number + " ";
            }
            if(result.Length > 1)
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }

        private string FormatList(List<string> list)
        {
            string result = "";
            if (list.Count == 0)
                return result;
            foreach (var number in list)
            {
                result += number + " ";
            }
            if (result.Length > 1)
            {
                result = result.Substring(0, result.Length - 1);
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
            //int index = 1;
            //while(File.Exists(filename))
            //{
            //    filename = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), String.Format("screenshot-{0}.jpg", index));
            //    index++;
            //}
            
            screenshot.Bitmap.Save(filename, ImageFormat.Jpeg);

            _ocr.Process(filename);

            GameStateDetector.Update(screenshot);
        }

        private void uxClear_Click(object sender, EventArgs e)
        {
            uxStatus.Text = String.Empty;
        }
    }
}

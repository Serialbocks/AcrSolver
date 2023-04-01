using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace AcrSolver
{
    public class OCR
    {
        public delegate void PrintInfoDelegate(string text);
        public delegate void NotifyCompleteDelegate();

        private const string _pythonPath = "../../../ocr.py";
        private const string _ocrOutFile = "ocr.json";

        public enum OCRState
        {
            Initializing,
            Ready,
            Processing
        }

        private Process _ocrProcess;
        private OCRState _state;
        private object stateLock = new object();
        private PrintInfoDelegate _printInfo;
        private NotifyCompleteDelegate _notifycomplete;
        private OCRState State { get
            {
                lock(stateLock)
                {
                    return _state;
                }
            }
            set
            {
                lock(stateLock)
                {
                    _state = value;
                }
            }
        }

        public OCR(PrintInfoDelegate printInfo, NotifyCompleteDelegate notifyComplete)
        {
            State = OCRState.Initializing;
            _printInfo = printInfo;
            _notifycomplete = notifyComplete;

            _printInfo("OCR Initializing...");
            _ocrProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "python",
                    Arguments = _pythonPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                },
            };
            _ocrProcess.EnableRaisingEvents = false;
            _ocrProcess.OutputDataReceived += (sender, args) => HandleStdout(args.Data);
            _ocrProcess.ErrorDataReceived += (sender, args) => HandleStdout(args.Data);
            _ocrProcess.Start();
            _ocrProcess.BeginOutputReadLine();
            _ocrProcess.BeginErrorReadLine();
        }

        public void Process(string file)
        {
            if(State != OCRState.Ready)
            {
                _printInfo("Tried to process OCR in an unready state");
                return;
            }
            _printInfo("OCR Processing...");
            _ocrProcess.StandardInput.WriteLine(file);
        }

        public void Stop()
        {
            if(_ocrProcess != null && !_ocrProcess.HasExited)
            {
                _ocrProcess.Kill();
            }
        }

        private BoundingBox _totalBox = new BoundingBox
        {
            X1 = 0.43111f,
            X2 = 0.577452f,
            Y1 = 0.370747f,
            Y2 = 0.430977f
        };

        private BoundingBox GetBoundingBoxFromGeometry(JProperty geometry)
        {
            var topLeft = geometry.First.First;
            var bottomRight = geometry.First.Last;
            return new BoundingBox
            {
                X1 = topLeft.First.Value<float>(),
                Y1 = topLeft.Last.Value<float>(),
                X2 = bottomRight.First.Value<float>(),
                Y2 = bottomRight.Last.Value<float>()
            };
        }

        private void ParseOCRData()
        {
            float ProcessLine(JToken line, BoundingBox targetBox)
            {
                var lineGeo = (JProperty)line.First;
                var lineBoundingBox = GetBoundingBoxFromGeometry(lineGeo);
                if (lineBoundingBox.IsWithin(targetBox))
                {
                    var words = line.Children().First(x => ((JProperty)x).Name == "words").First;
                    foreach (var word in words)
                    {
                        var wordValue = word.Children()
                            .First(x => ((JProperty)x).Name == "value").First
                            .Value<string>()
                            .Trim();
                        if (wordValue.Contains("$") || wordValue.Contains("BB"))
                        {
                            var strippedValue = wordValue
                                .ToUpper()
                                .Replace("TOTAL", "")
                                .Replace(":", "")
                                .Replace("$", "")
                                .Replace("BB", "")
                                .Trim();
                            float result;
                            if (float.TryParse(strippedValue, out result))
                            {
                                if(wordValue.Contains("BB"))
                                {
                                    GameState.AmountsInBB = true;
                                }
                                else
                                {
                                    GameState.AmountsInBB = false;
                                }
                                return result;
                            }
                        }
                    }
                }
                return 0;
            }

            void ProcessBlock(JToken blockObj)
            {
                var geometry = (JProperty)blockObj.First;
                var boundingBox = GetBoundingBoxFromGeometry(geometry);
                var lines = blockObj.Children().First(x => ((JProperty)x).Name == "lines").First;
                foreach (var seat in GameState.Seats)
                {
                    if(seat.StackBoundingBox != null && boundingBox.IsWithin(seat.StackBoundingBox))
                    {
                        foreach (var line in lines)
                        {
                            var result = ProcessLine(line, seat.StackBoundingBox);
                            if (result > 0)
                                seat.Stack = result;
                        }
                    }
                    else if (seat.BetBoundingBox != null && boundingBox.IsWithin(seat.BetBoundingBox))
                    {
                        foreach (var line in lines)
                        {
                            var result = ProcessLine(line, seat.BetBoundingBox);
                            if (result > 0)
                                seat.Bet = result;
                        }
                    }
                }

                foreach (var line in lines)
                {
                    var totalResult = ProcessLine(line, _totalBox);
                    if (totalResult > 0)
                    {
                        GameState.Total = totalResult;
                        break;
                    }
                }
            }

            GameState.ClearSeatBets();

            var jObject = JObject.Parse(File.ReadAllText(_ocrOutFile));
            var pages = jObject.First;
            var page = pages.First;
            var blocks = page
                .Children()
                .First()
                .Children()
                .First(x => ((JProperty)x).Name == "blocks")
                .First;
            foreach(var blockObj in blocks.Children())
            {
                ProcessBlock(blockObj);
            }

            GameState.UpdateCurrentBets();
        }

        private void HandleStdout(string text)
        {
            switch (text)
            {
                case "Ready":
                    State = OCRState.Ready;
                    _printInfo("OCR Ready!");
                    break;
                case "Done":
                    ParseOCRData();
                    State = OCRState.Ready;
                    _notifycomplete();
                    _printInfo("OCR Done!");
                    break;
                default:
                    break;
            }
        }
    }
}

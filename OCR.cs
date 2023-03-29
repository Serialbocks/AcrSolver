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
        private PrintInfoDelegate PrintInfo { get; set; }
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

        public OCR(PrintInfoDelegate printInfo)
        {
            State = OCRState.Initializing;
            PrintInfo = printInfo;

            PrintInfo("OCR Initializing...");
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
                PrintInfo("Tried to process OCR in an unready state");
                return;
            }
            PrintInfo("OCR Processing...");
            _ocrProcess.StandardInput.WriteLine(file);
        }

        public void Stop()
        {
            if(_ocrProcess != null && !_ocrProcess.HasExited)
            {
                _ocrProcess.Kill();
            }
        }

        public class Seat
        {
            public float Bet { get; set; }
            public float Stack { get; set; }
            public BoundingBox BetBoundingBox { get; set; }
            public BoundingBox StackBoundingBox { get; set; }
        }

        private List<Seat> _seats = new List<Seat>
        {
            new Seat
            {
                BetBoundingBox = new BoundingBox
                {
                    X1 = 0.39079f,
                    X2 = 0.642398f,
                    Y1 = 0.59339f,
                    Y2 = 0.77155f
                },
                StackBoundingBox = new BoundingBox
                {
                    X1 = 0.44968f,
                    X2 = 0.592077f,
                    Y1 = 0.818966f,
                    Y2 = 0.91092f
                }
            },
            new Seat
            {
                BetBoundingBox = new BoundingBox
                {
                    X1 = 0.11135f,
                    X2 = 0.384368f,
                    Y1 = 0.497126f,
                    Y2 = 0.75575f
                },
                StackBoundingBox = new BoundingBox
                {
                    X1 = 0.157388f,
                    X2 = 0.304069f,
                    Y1 = 0.739943f,
                    Y2 = 0.824713f
                }
            },
            new Seat
            {
                BetBoundingBox = new BoundingBox
                {
                    X1 = 0.09850f,
                    X2 = 0.34475f,
                    Y1 = 0.23707f,
                    Y2 = 0.50431f
                },
                StackBoundingBox = new BoundingBox
                {
                    X1 = 0.0642398f,
                    X2 = 0.208779f,
                    Y1 = 0.275862f,
                    Y2 = 0.3706897f
                }
            },
            new Seat
            {
                BetBoundingBox = new BoundingBox
                {
                    X1 = 0.344754f,
                    X2 = 0.6541756f,
                    Y1 = 0.1781609f,
                    Y2 = 0.382184f
                },
                StackBoundingBox = new BoundingBox
                {
                    X1 = 0.427195f,
                    X2 = 0.572805f,
                    Y1 = 0.193966f,
                    Y2 = 0.2801724f
                }
            },
            new Seat
            {
                BetBoundingBox = new BoundingBox
                {
                    X1 = 0.641328f,
                    X2 = 0.8803419f,
                    Y1 = 0.25f,
                    Y2 = 0.5086207f
                },
                StackBoundingBox = new BoundingBox
                {
                    X1 = 0.79015f,
                    X2 = 0.9346895f,
                    Y1 = 0.2744253f,
                    Y2 = 0.357759f
                }
            },
            new Seat
            {
                StackBoundingBox = new BoundingBox
                {
                    X1 = 0.697002f,
                    X2 = 0.8415418f,
                    Y1 = 0.741379f,
                    Y2 = 0.823276f
                },
                BetBoundingBox = new BoundingBox
                {
                    X1 = 0.6327623f,
                    X2 = 0.920771f,
                    Y1 = 0.49713f,
                    Y2 = 0.7428161f
                }
            }
        };

        public class BoundingBox
        {
            public float X1 { get; set; }
            public float Y1 { get; set; }
            public float X2 { get; set; }
            public float Y2 { get; set; }

            public bool IsWithin(BoundingBox box)
            {
                return X1 > box.X1 && X2 < box.X2 && Y1 > box.Y1 && Y2 < box.Y2;
            }
        }

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
                            var strippedValue = wordValue.Replace("$", "").Replace("BB", "");
                            float result;
                            if (float.TryParse(strippedValue, out result))
                            {
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
                foreach(var seat in _seats)
                {
                    var lines = blockObj.Children().First(x => ((JProperty)x).Name == "lines").First;
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

            }

            foreach (var seat in _seats)
            {
                seat.Bet = 0;
            }

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

            var index = 1;
            foreach(var seat in _seats)
            {
                if(seat.Bet > 0)
                {
                    PrintInfo(String.Format("Seat {0} bet ${1}", index, seat.Bet));
                }
                index++;
            }
        }

        private void HandleStdout(string text)
        {
            switch (text)
            {
                case "Ready":
                    State = OCRState.Ready;
                    PrintInfo("OCR Ready!");
                    break;
                case "Done":
                    ParseOCRData();
                    State = OCRState.Ready;
                    PrintInfo("OCR Done!");
                    break;
                default:
                    break;
            }
        }
    }
}

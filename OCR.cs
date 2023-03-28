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

        private readonly BoundingBox _seat3BetBoundingBox = new BoundingBox
        {
            X1 = 0.09850f,
            X2 = 0.34475f,
            Y1 = 0.23707f,
            Y2 = 0.50431f
        };

        private readonly BoundingBox _seat3StackBoundingBox = new BoundingBox
        {
            X1 = 0.0642398f,
            X2 = 0.208779f,
            Y1 = 0.275862f,
            Y2 = 0.3706897f
        };

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

        private void ReadOCRData()
        {
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
                var geometry = (JProperty)blockObj.First;
                var boundingBox = GetBoundingBoxFromGeometry(geometry);
                if(boundingBox.IsWithin(_seat3BetBoundingBox))
                {
                    var lines = blockObj.Children().First(x => ((JProperty)x).Name == "lines").First;
                    foreach(var line in lines)
                    {
                        var lineGeo = (JProperty)line.First;
                        var lineBoundingBox = GetBoundingBoxFromGeometry(lineGeo);
                        if(lineBoundingBox.IsWithin(_seat3BetBoundingBox) && !lineBoundingBox.IsWithin(_seat3StackBoundingBox))
                        {
                            var words = line.Children().First(x => ((JProperty)x).Name == "words").First;
                            foreach(var word in words)
                            {
                                var wordValue = word.Children()
                                    .First(x => ((JProperty)x).Name == "value").First
                                    .Value<string>();
                                if(wordValue.Contains("$") || wordValue.Contains("BB"))
                                {
                                    PrintInfo(wordValue);
                                }
                            }
                        }
                    }
                }
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
                    ReadOCRData();
                    State = OCRState.Ready;
                    PrintInfo("OCR Done!");
                    break;
                default:
                    break;
            }
        }
    }
}

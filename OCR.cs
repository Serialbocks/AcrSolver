using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace AcrSolver
{
    public class OCR
    {
        public delegate void PrintInfoDelegate(string text);

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
                    Arguments = "../../../ocr.py",
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

        private void HandleStdout(string text)
        {
            switch (text)
            {
                case "Ready":
                    State = OCRState.Ready;
                    PrintInfo("OCR Ready!");
                    break;
                case "Done":
                    State = OCRState.Ready;
                    PrintInfo("OCR Done!");
                    break;
                default:
                    break;
            }
        }
    }
}

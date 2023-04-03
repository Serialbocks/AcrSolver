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
    public class TexasSolver
    {
        public delegate void PrintInfoDelegate(string text);
        public delegate void NotifyCompleteDelegate();
        public TexasSolverState State
        {
            get
            {
                lock (stateLock)
                {
                    return _state;
                }
            }
            set
            {
                lock (stateLock)
                {
                    _state = value;
                }
            }
        }

        private const string _solverPath = "console_solver.exe";

        private PrintInfoDelegate _printInfo;
        private NotifyCompleteDelegate _notifyComplete;
        private Process _solverProcess { get; set; }
        private TexasSolverState _state;
        private object stateLock = new object();

        private PreflopRanges _preflopRanges;

        public enum TexasSolverState
        {
            Initializing,
            Ready,
            Processing
        }

        public TexasSolver(PrintInfoDelegate printInfo, NotifyCompleteDelegate notifyComplete)
        {
            _printInfo = printInfo;
            _notifyComplete = notifyComplete;
            State = TexasSolverState.Initializing;

            _preflopRanges = new PreflopRanges();
            _printInfo("Solver Initializing...");
            _solverProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = _solverPath,
                    Arguments = String.Empty,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                },
            };
            _solverProcess.EnableRaisingEvents = false;
            _solverProcess.OutputDataReceived += (sender, args) => HandleStdout(args.Data);
            _solverProcess.ErrorDataReceived += (sender, args) => HandleStdout(args.Data);
            _solverProcess.Start();
            _solverProcess.BeginOutputReadLine();
            _solverProcess.BeginErrorReadLine();
        }

        public void Stop()
        {
            if (_solverProcess != null && !_solverProcess.HasExited)
            {
                _solverProcess.Kill();
            }
        }

        public void ShowHandRange()
        {
            if(GameState.PlayerHand.Count == 2)
            {
                var playerRange = _preflopRanges.GetRange(GameState.PlayerPosition(), GameState.Bets);
                if(playerRange != null)
                {
                    var handRange = playerRange.GetRangeHand(GameState.PlayerHand);
                    if (handRange != null)
                        _printInfo(handRange.ToString());
                }

            }
        }

        private void HandleStdout(string text)
        {
            if (State == TexasSolverState.Initializing)
            {
                State = TexasSolverState.Ready;
                _printInfo("Solver ready!");
            }
        }
    }
}

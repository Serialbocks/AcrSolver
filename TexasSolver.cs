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

        private const int _threadCount = 32;
        private const float _accuracy = 1.0f;

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

        public void Solve()
        {
            _solverProcess.StandardInput.WriteLine(String.Format("set_pot {0}", GameState.Pot));

            // Set effective stack
            float effectiveStack = float.MaxValue;
            foreach(var seat in GameState.Seats)
            {
                if(seat.HasCards && seat.Stack > 0 && seat.Stack < effectiveStack)
                {
                    effectiveStack = seat.Stack;
                }
            }
            _solverProcess.StandardInput.WriteLine(String.Format("set_effective_stack {0}", effectiveStack));

            // Set flop
            if(GameState.Board.Count != 3)
            {
                _printInfo("Tried to solve without full flop");
                return;
            }
            var flopStr = String.Format("{0},{1},{2}", GameState.Board[0], GameState.Board[1], GameState.Board[2]);
            flopStr = flopStr.Replace("10", "T");
            _solverProcess.StandardInput.WriteLine(String.Format("set_board {0}", flopStr));

            // Set ranges
            Seat oop = null;
            Seat ip = null;
            var playersWithCards = GameState.Seats.Where(x => x.HasCards).ToList();
            if(playersWithCards.Count != 2)
            {
                _printInfo(String.Format("Tried to solve with {0} players", playersWithCards.Count));
                return;
            }
            if(GameState.PostflopOrder(playersWithCards[0].Position) > GameState.PostflopOrder(playersWithCards[1].Position))
            {
                ip = playersWithCards[0];
                oop = playersWithCards[1];
            }
            else
            {
                ip = playersWithCards[1];
                oop = playersWithCards[0];
            }
            if(GameState.PreflopBets.Count == 0)
            {
                _printInfo(String.Format("No preflop bets to solve for!"));
                return;
            }
            var lastBet = GameState.PreflopBets[GameState.PreflopBets.Count - 1];
            var ipBetLast = true;
            if(lastBet.Seat == ip)
            {
                ipBetLast = true;
            }
            else if(lastBet.Seat == oop)
            {
                ipBetLast = false;
            }
            else
            {
                _printInfo("Last preflop bet doesn't match either player");
                return;
            }

            var previousPreflopBets = new List<Bet>(GameState.PreflopBets);
            previousPreflopBets.RemoveAt(previousPreflopBets.Count - 1);
            Range ipRanges = null;
            Range oopRanges = null;
            if(ipBetLast)
            {
                ipRanges = _preflopRanges.GetRange(ip.Position, previousPreflopBets);
                oopRanges = _preflopRanges.GetRange(oop.Position, GameState.PreflopBets);
            }
            else
            {
                ipRanges = _preflopRanges.GetRange(ip.Position, GameState.PreflopBets);
                oopRanges = _preflopRanges.GetRange(oop.Position, previousPreflopBets);
            }
            if(ipRanges == null)
            {
                _printInfo("Could not find IP range");
                return;
            }
            
            if(oopRanges == null)
            {
                _printInfo("Could not find OOP range");
                return;
            }

            string ipRangeStr = String.Empty;
            string oopRangeStr = String.Empty;
            if(ipBetLast)
            {
                ipRangeStr = ipRanges.RaiseRange;
                oopRangeStr = oopRanges.CallRange;
            }
            else
            {
                oopRangeStr = oopRanges.RaiseRange;
                ipRangeStr = ipRanges.CallRange;
            }
            _solverProcess.StandardInput.WriteLine(String.Format("set_range_ip {0}", ipRangeStr));
            _solverProcess.StandardInput.WriteLine(String.Format("set_range_oop {0}", oopRangeStr));

            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes oop,flop,bet,50"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes oop,flop,raise,60"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes ip,flop,bet,50"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes ip,flop,raise,60"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes oop,turn,bet,50"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes oop,turn,raise,60"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes ip,turn,bet,50"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes ip,turn,raise,60"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes oop,river,bet,50"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes oop,river,donk,50"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes oop,river,raise,60"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes ip,river,bet,50"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_bet_sizes ip,river,raise,60"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_allin_threshold 1.0"));
            _solverProcess.StandardInput.WriteLine(String.Format("build_tree"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_thread_num {0}", _threadCount));
            _solverProcess.StandardInput.WriteLine(String.Format("set_accuracy {0}", _accuracy));
            _solverProcess.StandardInput.WriteLine(String.Format("set_max_iteration 200"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_print_interval 10"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_use_isomorphism 1"));
            _solverProcess.StandardInput.WriteLine(String.Format("start_solve"));
            _solverProcess.StandardInput.WriteLine(String.Format("set_dump_rounds 2"));
            _solverProcess.StandardInput.WriteLine(String.Format("dump_result output_result.json"));

            _printInfo("Solver processing...");
            GameState.SolvedThisFlop = true;
            State = TexasSolverState.Processing;
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
            else
            {
                _printInfo(text);
            }
        }
    }
}

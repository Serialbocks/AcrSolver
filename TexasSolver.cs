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
    public class RangeHand
    {
        public float FoldFrequency { get; set; }
        public float CallFrequency { get; set; }
        public float RaiseFrequency { get; set; }
    }

    public class Range
    {
        private Dictionary<string, RangeHand> _range = new Dictionary<string, RangeHand>();
        private static List<string> _cardValues = new List<string> { "A", "K", "Q", "J", "T", "9", "8", "7", "6", "5", "4", "3", "2" };
        public Range(string foldFilename, string callFilename, string raiseFilename)
        {
            for (int i = 0; i < _cardValues.Count; i++)
            {
                for (int k = i; k < _cardValues.Count; k++)
                {
                    var card1 = _cardValues[i];
                    var card2 = _cardValues[k];
                    var hand = card1 + card2;
                    if (card1 == card2)
                    {
                        _range.Add(hand, new RangeHand());
                    }
                    else
                    {
                        _range.Add(hand + "s", new RangeHand());
                        _range.Add(hand + "o", new RangeHand());
                    }
                }
            }

            if (!string.IsNullOrEmpty(foldFilename))
            {
                var foldValues = File.ReadAllText(foldFilename).Split(',');
                foreach (var foldValue in foldValues)
                {
                    var foldSplit = foldValue.Split(":");
                    var hand = foldSplit[0];
                    var frequency = float.Parse(foldSplit[1]);
                    _range[hand].FoldFrequency = frequency;
                }
            }
            if (!string.IsNullOrEmpty(callFilename))
            {
                var callValues = File.ReadAllText(callFilename).Split(',');
                foreach (var callValue in callValues)
                {
                    var callSplit = callValue.Split(":");
                    var hand = callSplit[0];
                    var frequency = float.Parse(callSplit[1]);
                    _range[hand].CallFrequency = frequency;
                }
            }
            if (!string.IsNullOrEmpty(raiseFilename))
            {
                var raiseValues = File.ReadAllText(raiseFilename).Split(',');
                foreach (var raiseValue in raiseValues)
                {
                    var raiseSplit = raiseValue.Split(":");
                    var hand = raiseSplit[0];
                    var frequency = float.Parse(raiseSplit[1]);
                    _range[hand].RaiseFrequency = frequency;
                }
            }
        }
    }

    public class PreflopRangeNode
    {

        public Range Range { get; set; }

        public PreflopRangeNode UTGRaise { get; set; }
        public PreflopRangeNode UTGCall { get; set; }
        public PreflopRangeNode UTGFold { get; set; }
        public PreflopRangeNode MPRaise { get; set; }
        public PreflopRangeNode MPCall { get; set; }
        public PreflopRangeNode MPFold { get; set; }
        public PreflopRangeNode CORaise { get; set; }
        public PreflopRangeNode COCall { get; set; }
        public PreflopRangeNode COFold { get; set; }
        public PreflopRangeNode BTNRaise { get; set; }
        public PreflopRangeNode BTNCall { get; set; }
        public PreflopRangeNode BTNFold { get; set; }
        public PreflopRangeNode SBRaise { get; set; }
        public PreflopRangeNode SBCall { get; set; }
        public PreflopRangeNode SBFold { get; set; }
        public PreflopRangeNode BBRaise { get; set; }
        public PreflopRangeNode BBCall { get; set; }
        public PreflopRangeNode BBFold { get; set; }
    }

    public class PreflopRanges
    {
        public Dictionary<Position, PreflopRangeNode> _preflopRanges = new Dictionary<Position, PreflopRangeNode>();

        private const string _rangesPath = "ranges/qb_ranges/100bb 2.5x 500rake";

        public PreflopRanges()
        {
            

        }

        private void PopulatePositionRanges(Position position)
        {
            var topPath = Path.Combine(_rangesPath, position.ToString());
            var threeBetPath = Path.Combine(topPath, "vs_3bet");
            var fourBetPath = Path.Combine(topPath, "vs_4bet");
            var fiveBetPath = Path.Combine(topPath, "vs_5bet");

            var positionRoot = new PreflopRangeNode();
            _preflopRanges[position] = positionRoot;
            PopulateFolderRanges(position, topPath, positionRoot);
            PopulateFolderRanges(position, threeBetPath, positionRoot);
            PopulateFolderRanges(position, fourBetPath, positionRoot);
            PopulateFolderRanges(position, fiveBetPath, positionRoot);
        }

        private void PopulateFolderRanges(Position position, string folder, PreflopRangeNode root)
        {
            foreach(var file in Directory.GetFiles(folder))
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                var currentNode = root;
                var fileSplit = filename.Split("_");

                var splitIndex = 0;
                while(splitIndex < fileSplit.Length - 2)
                {
                    var targetPos = fileSplit[splitIndex];
                    var action = fileSplit[splitIndex + 1];



                    splitIndex += 2;
                }
            }
        }

    }

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

        private void LoadRanges()
        {

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

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
        public PreflopRangeNode BTNRanges { get; set; } = new PreflopRangeNode();
        public PreflopRangeNode SBRanges { get; set; } = new PreflopRangeNode();
        public PreflopRangeNode BBRanges { get; set; } = new PreflopRangeNode();
        public PreflopRangeNode UTGRanges { get; set; } = new PreflopRangeNode();
        public PreflopRangeNode MPRanges { get; set; } = new PreflopRangeNode();
        public PreflopRangeNode CORanges { get; set; } = new PreflopRangeNode();

        private const string _rangesPath = "ranges/qb_ranges/100bb 2.5x 500rake";

        public PreflopRanges()
        {
            BuildBTNRangeTree();
        }

        private void BuildBTNRangeTree()
        {
            var topPath = Path.Combine(_rangesPath, "BTN");
            var threeBetPath = Path.Combine(topPath, "vs_3bet");
            var fourBetPath = Path.Combine(topPath, "vs_4bet");
            var fiveBetPath = Path.Combine(topPath, "vs_5bet");

            // Open range
            BTNRanges.Range = new Range(Path.Combine(topPath, "BTN_FOLD.txt"), null, Path.Combine(topPath, "BTN_2.5bb.txt"));
            BTNRanges.BTNRaise = new PreflopRangeNode
            {
                // SB 3 bet after open
                SBRaise = new PreflopRangeNode
                {
                    Range = new Range(
                        Path.Combine(threeBetPath, "BTN_2.5bb_SB_11.0bb_BTN_FOLD.txt"),
                        Path.Combine(threeBetPath, "BTN_2.5bb_SB_11.0bb_BTN_Call.txt"),
                        Path.Combine(threeBetPath, "BTN_2.5bb_SB_11.0bb_BTN_24.0bb.txt")),
                    BTNRaise = new PreflopRangeNode
                    {
                        // SB 5 bet/shove
                        SBRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(fiveBetPath, "BTN_2.5bb_SB_11.0bb_BTN_24.0bb_SB_AllIn_BTN_FOLD.txt"),
                                Path.Combine(fiveBetPath, "BTN_2.5bb_SB_11.0bb_BTN_24.0bb_SB_AllIn_BTN_Call.txt"),
                                null),
                        }
                    },
                    BBRaise = new PreflopRangeNode
                    {
                        Range = new Range(Path.Combine(fourBetPath, "BTN_2.5bb_SB_11.0bb_BB_22.0bb_BTN_FOLD.txt"),
                            null,
                            Path.Combine(fourBetPath, "BTN_2.5bb_SB_11.0bb_BB_22.0bb_BTN_AllIn.txt")),
                    }
                },

                // BB 3 bet after open
                BBRaise = new PreflopRangeNode
                {
                    Range = new Range(
                        Path.Combine(threeBetPath, "BTN_2.5bb_BB_11.0bb_BTN_FOLD.txt"),
                        Path.Combine(threeBetPath, "BTN_2.5bb_BB_11.0bb_BTN_Call.txt"),
                        Path.Combine(threeBetPath, "BTN_2.5bb_BB_11.0bb_BTN_24.0bb.txt")),
                    BTNRaise = new PreflopRangeNode
                    {
                        // BB 5 bet/shove
                        BBRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(fiveBetPath, "BTN_2.5bb_BB_11.0bb_BTN_24.0bb_BB_AllIn_BTN_FOLD.txt"),
                                Path.Combine(fiveBetPath, "BTN_2.5bb_BB_11.0bb_BTN_24.0bb_BB_AllIn_BTN_Call.txt"),
                                null),
                        }
                    }
                }
            };

            // UTG Open
            BTNRanges.UTGRaise = new PreflopRangeNode
            {
                Range = new Range(
                            Path.Combine(topPath, "UTG_2.5bb_BTN_FOLD.txt"),
                            Path.Combine(topPath, "UTG_2.5bb_BTN_Call.txt"),
                            Path.Combine(topPath, "UTG_2.5bb_BTN_8.5bb.txt")),

                MPRaise = new PreflopRangeNode
                {
                    Range = new Range(
                        Path.Combine(threeBetPath, "UTG_2.5bb_MP_8.5bb_BTN_FOLD.txt"),
                        null,
                        Path.Combine(threeBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb.txt")),
                    CORaise = new PreflopRangeNode
                    {
                        Range = new Range(
                                    Path.Combine(fourBetPath, "UTG_2.5bb_MP_8.5bb_CO_20.0bb_BTN_FOLD.txt"),
                                    null,
                                    Path.Combine(fourBetPath, "UTG_2.5bb_MP_8.5bb_CO_20.0bb_BTN_AllIn.txt")),
                    },
                    BTNRaise = new PreflopRangeNode
                    {
                        MPRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_FOLD_MP_AllIn_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_FOLD_MP_AllIn_BTN_Call.txt"),
                                        null),
                        },
                        BBRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                        null),
                            MPCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_Call_BTN_Call.txt"),
                                        null),
                            },
                            UTGCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_MP_FOLD_BTN_Call.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_MP_FOLD_BTN_FOLD.txt"),
                                        null),
                                MPCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_MP_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_MP_Call_BTN_Call.txt"),
                                        null),
                                },
                                MPFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_MP_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_MP_FOLD_BTN_Call.txt"),
                                        null),
                                },
                            },
                            UTGFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                        null),
                                MPCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_Call_BTN_Call.txt"),
                                        null),
                                },
                                MPFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                        null),
                                },
                            },
                        },
                        SBRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                            null),
                            MPCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_Call_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_Call_BTN_Call.txt"),
                                            null),
                            },
                            BBCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                            null),
                                MPCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_Call_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_Call_BTN_Call.txt"),
                                            null),
                                },
                                MPFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                            null),
                                },
                                UTGCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_MP_FOLD_BTN_Call.txt"),
                                            null),
                                    MPCall = new PreflopRangeNode
                                    {
                                        Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_MP_Call_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_MP_Call_BTN_Call.txt"),
                                            null),
                                    },
                                    MPFold = new PreflopRangeNode
                                    {
                                        Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_MP_FOLD_BTN_Call.txt"),
                                            null),
                                    },
                                },
                                UTGFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                            null),
                                    MPCall = new PreflopRangeNode
                                    {
                                        Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_Call_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_Call_BTN_Call.txt"),
                                            null),
                                    },
                                    MPFold = new PreflopRangeNode
                                    {
                                        Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                            null),
                                    },
                                },
                            },
                            UTGCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_MP_FOLD_BTN_Call.txt"),
                                            null),
                                MPCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_MP_Call_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_MP_Call_BTN_Call.txt"),
                                            null),
                                },
                                MPFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_MP_FOLD_BTN_Call.txt"),
                                            null),
                                },
                            },
                            UTGFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                            null),
                                MPCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_Call_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_Call_BTN_Call.txt"),
                                            null),
                                },
                                MPFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_MP_FOLD_BTN_Call.txt"),
                                            null),
                                },
                            },
                        },
                        UTGRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_AllIn_MP_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_AllIn_MP_FOLD_BTN_Call.txt"),
                                        null),
                            MPCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_AllIn_MP_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_AllIn_MP_Call_BTN_Call.txt"),
                                        null),
                            },
                            MPFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_AllIn_MP_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_AllIn_MP_FOLD_BTN_Call.txt"),
                                        null),
                            },
                        },
                        UTGFold = new PreflopRangeNode
                        {
                            MPRaise = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_FOLD_MP_AllIn_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_MP_8.5bb_BTN_20.0bb_UTG_FOLD_MP_AllIn_BTN_Call.txt"),
                                        null),
                            },
                        },
                    },
                },

                CORaise = new PreflopRangeNode
                {
                    Range = new Range(
                        Path.Combine(threeBetPath, "UTG_2.5bb_CO_8.5bb_BTN_FOLD.txt"),
                        null,
                        Path.Combine(threeBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb.txt")),
                    BTNRaise = new PreflopRangeNode
                    {
                        CORaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_FOLD_CO_AllIn_BTN_Call.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_FOLD_CO_AllIn_BTN_Call.txt"),
                                        null),
                        },
                        BBRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                        null),
                            COCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_Call_BTN_Call.txt"),
                                        null),
                            },
                            COFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                        null),
                            },
                            UTGCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_CO_FOLD_BTN_Call.txt"),
                                        null),
                                COCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_CO_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_CO_Call_BTN_Call.txt"),
                                        null),
                                },
                                COFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_Call_CO_FOLD_BTN_Call.txt"),
                                        null),
                                }
                            },
                            UTGFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                        null),
                                COCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_Call_BTN_Call.txt"),
                                        null),
                                },
                                COFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_BB_AllIn_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                        null),
                                }
                            }
                        },
                        SBRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                        null),
                            COCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_Call_BTN_Call.txt"),
                                        null),
                            },
                            COFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                        null),
                            },
                            BBCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                            null),
                                COCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_Call_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_Call_BTN_Call.txt"),
                                            null),
                                },
                                COFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                            null),
                                },
                                UTGCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_CO_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_CO_FOLD_BTN_Call.txt"),
                                            null),
                                    COCall = new PreflopRangeNode
                                    {
                                        Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_CO_Call_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_CO_Call_BTN_Call.txt"),
                                            null),
                                    },
                                    COFold = new PreflopRangeNode
                                    {
                                        Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_CO_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_Call_CO_FOLD_BTN_Call.txt"),
                                            null),
                                    }
                                },
                                UTGFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                            null),
                                    COCall = new PreflopRangeNode
                                    {
                                        Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_Call_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_Call_BTN_Call.txt"),
                                            null),
                                    },
                                    COFold = new PreflopRangeNode
                                    {
                                        Range = new Range(
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                            Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_BB_Call_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                            null),
                                    }
                                }
                            },
                            UTGCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_CO_FOLD_BTN_Call.txt"),
                                        null),
                                COCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_CO_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_CO_Call_BTN_Call.txt"),
                                        null),
                                },
                                COFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_Call_CO_FOLD_BTN_Call.txt"),
                                        null),
                                }
                            },
                            UTGFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                        null),
                                COCall = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_Call_BTN_Call.txt"),
                                        null),
                                },
                                COFold = new PreflopRangeNode
                                {
                                    Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_SB_AllIn_UTG_FOLD_CO_FOLD_BTN_Call.txt"),
                                        null),
                                },
                            }
                        },
                        UTGRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_AllIn_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_AllIn_CO_FOLD_BTN_Call.txt"),
                                        null),
                            COCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_AllIn_CO_Call_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_AllIn_CO_Call_BTN_Call.txt"),
                                        null),
                            },
                            COFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_AllIn_CO_FOLD_BTN_FOLD.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_AllIn_CO_FOLD_BTN_Call.txt"),
                                        null),
                            }
                        },
                        UTGFold = new PreflopRangeNode
                        {
                            CORaise = new PreflopRangeNode
                            {
                                Range = new Range(
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_FOLD_CO_AllIn_BTN_Call.txt"),
                                        Path.Combine(fiveBetPath, "UTG_2.5bb_CO_8.5bb_BTN_20.0bb_UTG_FOLD_CO_AllIn_BTN_Call.txt"),
                                        null),
                            },
                        },
                    },
                },

                BTNRaise = new PreflopRangeNode
                {
                    BBRaise = new PreflopRangeNode
                    {
                        Range = new Range(
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_BB_21.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_BB_21.0bb_UTG_FOLD_BTN_Call.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_BB_21.0bb_UTG_FOLD_BTN_AllIn.txt")),
                        UTGRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_8.5bb_BB_21.0bb_UTG_AllIn_BTN_FOLD.txt"),
                                Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_8.5bb_BB_21.0bb_UTG_AllIn_BTN_Call.txt"),
                                null),
                        },
                        UTGFold = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_BB_21.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_BB_21.0bb_UTG_FOLD_BTN_Call.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_BB_21.0bb_UTG_FOLD_BTN_AllIn.txt")),
                        }
                    },
                    SBRaise = new PreflopRangeNode
                    {
                        Range = new Range(
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_UTG_FOLD_BTN_Call.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_UTG_FOLD_BTN_AllIn.txt")),
                        UTGFold = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_UTG_FOLD_BTN_Call.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_UTG_FOLD_BTN_AllIn.txt")),
                        },
                        BBRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                    Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_BB_AllIn_UTG_FOLD_BTN_FOLD.txt"),
                                    Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_BB_AllIn_UTG_FOLD_BTN_Call.txt"),
                                    null),
                            UTGCall = new PreflopRangeNode
                            {
                                Range = new Range(
                                    Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_BB_AllIn_UTG_Call_BTN_FOLD.txt"),
                                    Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_BB_AllIn_UTG_Call_BTN_Call.txt"),
                                    null),
                            },
                            UTGFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                    Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_BB_AllIn_UTG_FOLD_BTN_FOLD.txt"),
                                    Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_8.5bb_SB_21.0bb_BB_AllIn_UTG_FOLD_BTN_Call.txt"),
                                    null),
                            }
                        }
                    },
                    UTGRaise = new PreflopRangeNode
                    {
                        Range = new Range(
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_UTG_22.0bb_BTN_FOLD.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_UTG_22.0bb_BTN_Call.txt"),
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_8.5bb_UTG_22.0bb_BTN_AllIn.txt")),
                    },
                },

                BTNCall = new PreflopRangeNode
                {
                    BBRaise = new PreflopRangeNode
                    {
                        Range = new Range(
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_FOLD_BTN_Call.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_FOLD_BTN_AllIn.txt")),
                        UTGRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_25.0bb_BTN_FOLD.txt"),
                                null,
                                Path.Combine(fourBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_25.0bb_BTN_AllIn.txt")),
                        },
                        UTGCall = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_Call_BTN_FOLD.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_Call_BTN_Call.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_Call_BTN_AllIn.txt")),
                        },
                        UTGFold = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_FOLD_BTN_Call.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_BB_13.0bb_UTG_FOLD_BTN_AllIn.txt")),
                        }
                    },
                    SBRaise = new PreflopRangeNode
                    {
                        Range = new Range(
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_FOLD_BTN_Call.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_FOLD_BTN_AllIn.txt")),
                        BBRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                    Path.Combine(fourBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_BB_25.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                    null,
                                    Path.Combine(fourBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_BB_25.0bb_UTG_FOLD_BTN_AllIn.txt")),
                            UTGFold = new PreflopRangeNode
                            {
                                Range = new Range(
                                    Path.Combine(fourBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_BB_25.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                    null,
                                    Path.Combine(fourBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_BB_25.0bb_UTG_FOLD_BTN_AllIn.txt")),
                            },
                            UTGRaise = new PreflopRangeNode
                            {
                                Range = new Range(
                                    Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_BB_25.0bb_UTG_AllIn_BTN_FOLD.txt"),
                                    Path.Combine(fiveBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_BB_25.0bb_UTG_AllIn_BTN_Call.txt"),
                                    null),
                            },
                        },
                        UTGRaise = new PreflopRangeNode
                        {
                            Range = new Range(
                                    Path.Combine(fourBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_25.0bb_BTN_FOLD.txt"),
                                    null,
                                    Path.Combine(fourBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_25.0bb_BTN_AllIn.txt")),
                        },
                        UTGCall = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_Call_BTN_FOLD.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_Call_BTN_Call.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_Call_BTN_AllIn.txt")),
                        },
                        UTGFold = new PreflopRangeNode
                        {
                            Range = new Range(
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_FOLD_BTN_FOLD.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_FOLD_BTN_Call.txt"),
                                Path.Combine(threeBetPath, "UTG_2.5bb_BTN_Call_SB_13.0bb_UTG_FOLD_BTN_AllIn.txt")),
                        }
                    }
                }
            };

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

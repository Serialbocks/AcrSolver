using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcrSolver
{
    public class RangeHand
    {
        public float FoldFrequency { get; set; }
        public float CallFrequency { get; set; }
        public float RaiseFrequency { get; set; }

        private static float NextFloat(float min, float max)
        {
            Random random = new Random();
            double val = (random.NextDouble() * (max - min) + min);
            return (float)val;
        }

        public override string ToString()
        {
            var rng = RangeHand.NextFloat(0, 1.0f);
            string action = String.Empty;
            if(rng < FoldFrequency)
            {
                action = "Fold";
            }
            else if(rng < FoldFrequency + CallFrequency)
            {
                action = "Call";
            }
            else
            {
                action = "Raise";
            }

            if(FoldFrequency == 0 && CallFrequency == 0 && RaiseFrequency == 0)
            {
                action = "Fold";
            }
            return String.Format("Call: {0} Raise: {1} Fold: {2} RNG: {3}", CallFrequency, RaiseFrequency, FoldFrequency, action);
        }
    }

    public class Range
    {
        public string FoldRange { get; set; }
        public string CallRange { get; set; }
        public string RaiseRange { get; set; }

        public float? RaiseVal { get; set; }

        private Dictionary<string, RangeHand> _range = new Dictionary<string, RangeHand>();
        private static List<string> _cardValues = new List<string> { "A", "K", "Q", "J", "T", "9", "8", "7", "6", "5", "4", "3", "2" };

        public Range()
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
                        var reversehand = card2 + card1;
                        _range.Add(hand + "s", new RangeHand());
                        _range.Add(hand + "o", new RangeHand());
                        _range.Add(reversehand + "s", new RangeHand());
                        _range.Add(reversehand + "o", new RangeHand());
                    }
                }
            }
        }

        public RangeHand GetRangeHand(List<string> hand)
        {
            var handType = String.Empty;
            var card1 = hand[0];
            var card2 = hand[1];
            if(card1.Substring(0, 2) == "10")
            {
                card1 = "T" + card1[card1.Length - 1];
            }
            if(card2.Substring(0, 2) == "10")
            {
                card2 = "T" + card2[card2.Length - 1];
            }
            var isPocketPair = card1[0] == card2[0];
            if(!isPocketPair && card1[1] == card2[1])
            {
                handType = "s";
            }
            else if(!isPocketPair)
            {
                handType = "o";
            }
            var handLookup = String.Format("{0}{1}{2}", card1[0], card2[0], handType);
            RangeHand rangeHand;
            if(!_range.TryGetValue(handLookup, out rangeHand))
            {
                throw new Exception(String.Format("Could not find hand {0}", handLookup));
            }
            return _range[handLookup];
        }

        public void SetFoldFrequency(string filename)
        {
            FoldRange = File.ReadAllText(filename);
            var foldValues = FoldRange.Split(',');
            foreach (var foldValue in foldValues)
            {
                var foldSplit = foldValue.Split(":");
                var hand = foldSplit[0];
                var frequency = float.Parse(foldSplit[1]);
                _range[hand].FoldFrequency = frequency;
            }
        }

        public void SetCallFrequency(string filename)
        {
            CallRange = File.ReadAllText(filename);
            var callValues = CallRange.Split(',');
            foreach (var callValue in callValues)
            {
                var callSplit = callValue.Split(":");
                var hand = callSplit[0];
                var frequency = float.Parse(callSplit[1]);
                _range[hand].CallFrequency = frequency;
            }
        }

        public void SetRaiseFrequency(string filename)
        {
            RaiseRange = File.ReadAllText(filename);
            var raiseValues = RaiseRange.Split(',');
            foreach (var raiseValue in raiseValues)
            {
                var raiseSplit = raiseValue.Split(":");
                var hand = raiseSplit[0];
                var frequency = float.Parse(raiseSplit[1]);
                _range[hand].RaiseFrequency = frequency;
            }
        }
    }

    public class PreflopRangeNode
    {

        public Range Range { get; set; }

        public float? RaiseVal { get; set; }
        public bool IsFoldNode { get; set; }

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

        public PreflopRangeNode ParentNode { get; set; }
    }

    public class PreflopRanges
    {
        public Dictionary<Position, PreflopRangeNode> _preflopRanges = new Dictionary<Position, PreflopRangeNode>();

        private const string _rangesPath = "ranges/qb_ranges/100bb 2.5x 500rake";

        public PreflopRanges()
        {
            foreach (var position in (Position[])Enum.GetValues(typeof(Position)))
            {
                PopulatePositionRanges(position);
            }
        }

        public Range GetRange(Position position, List<Bet> bets)
        {
            var currentNode = _preflopRanges[position];

            foreach(var bet in bets)
            {
                switch(bet.Type)
                {
                    case BetType.Bet:
                        currentNode = GetNodeFromPositionAction(bet.Position.ToString(), "Raise", currentNode);
                        break;
                    case BetType.Call:
                        currentNode = GetNodeFromPositionAction(bet.Position.ToString(), "Call", currentNode);
                        break;
                    case BetType.Raise:
                        currentNode = GetNodeFromPositionAction(bet.Position.ToString(), "Raise", currentNode);
                        break;
                    default:
                        throw new Exception("Invalid bet type");
                }
                if(currentNode == null)
                {
                    return null;
                }
            }

            return currentNode.Range;
        }

        private void PopulatePositionRanges(Position position)
        {
            var topPath = Path.Combine(_rangesPath, position.ToString());
            var threeBetPath = Path.Combine(topPath, "vs_3bet");
            var fourBetPath = Path.Combine(topPath, "vs_4bet");
            var fiveBetPath = Path.Combine(topPath, "vs_5bet");

            var positionRoot = new PreflopRangeNode();
            _preflopRanges[position] = positionRoot;
            PopulateFolderRanges(topPath, positionRoot);
            PopulateFolderRanges(threeBetPath, positionRoot);
            PopulateFolderRanges(fourBetPath, positionRoot);
            PopulateFolderRanges(fiveBetPath, positionRoot);

            BringUpFoldRanges(positionRoot);
        }

        private void BringUpFoldRanges(PreflopRangeNode node)
        {
            if (node == null)
                return;

            BringUpFoldRanges(node.UTGCall);
            BringUpFoldRanges(node.UTGFold);
            BringUpFoldRanges(node.UTGRaise);
            BringUpFoldRanges(node.MPCall);
            BringUpFoldRanges(node.MPFold);
            BringUpFoldRanges(node.MPRaise);
            BringUpFoldRanges(node.COCall);
            BringUpFoldRanges(node.COFold);
            BringUpFoldRanges(node.CORaise);
            BringUpFoldRanges(node.BTNCall);
            BringUpFoldRanges(node.BTNFold);
            BringUpFoldRanges(node.BTNRaise);
            BringUpFoldRanges(node.SBCall);
            BringUpFoldRanges(node.SBFold);
            BringUpFoldRanges(node.SBRaise);
            BringUpFoldRanges(node.BBCall);
            BringUpFoldRanges(node.BBFold);
            BringUpFoldRanges(node.BBRaise);

            if (!node.IsFoldNode || node.ParentNode == null)
            {
                return;
            }

            var parentNode = node.ParentNode;
            if (parentNode.Range == null)
                parentNode.Range = node.Range;

            if(parentNode.UTGCall == null)
                parentNode.UTGCall = node.UTGCall;
            if (parentNode.UTGFold == null)
                parentNode.UTGFold = node.UTGFold;
            if (parentNode.UTGRaise == null)
                parentNode.UTGRaise = node.UTGRaise;
            if (parentNode.MPCall == null)
                parentNode.MPCall = node.MPCall;
            if (parentNode.MPFold == null)
                parentNode.MPFold = node.MPFold;
            if (parentNode.MPRaise == null)
                parentNode.MPRaise = node.MPRaise;
            if (parentNode.COCall == null)
                parentNode.COCall = node.COCall;
            if (parentNode.COFold == null)
                parentNode.COFold = node.COFold;
            if (parentNode.CORaise == null)
                parentNode.CORaise = node.CORaise;
            if (parentNode.BTNCall == null)
                parentNode.BTNCall = node.BTNCall;
            if (parentNode.BTNFold == null)
                parentNode.BTNFold = node.BTNFold;
            if (parentNode.BTNRaise == null)
                parentNode.BTNRaise = node.BTNRaise;
            if (parentNode.SBCall == null)
                parentNode.SBCall = node.SBCall;
            if (parentNode.SBFold == null)
                parentNode.SBFold = node.SBFold;
            if (parentNode.SBRaise == null)
                parentNode.SBRaise = node.SBRaise;
            if (parentNode.BBCall == null)
                parentNode.BBCall = node.BBCall;
            if (parentNode.BBFold == null)
                parentNode.BBFold = node.BBFold;
            if (parentNode.BBRaise == null)
                parentNode.BBRaise = node.BBRaise;
        }


        private PreflopRangeNode GetNodeFromPositionAction(string position, string action, PreflopRangeNode root, bool forceNewIfNull = false)
        {
            PreflopRangeNode node = null;
            switch (position)
            {
                case "UTG":
                    switch (action)
                    {
                        case "Call":
                            if (forceNewIfNull && root.UTGCall == null)
                                root.UTGCall = new PreflopRangeNode();
                            node = root.UTGCall;
                            break;
                        case "FOLD":
                            if (forceNewIfNull && root.UTGFold == null)
                                root.UTGFold = new PreflopRangeNode() { IsFoldNode = true };
                            node = root.UTGFold;
                            break;
                        case "Raise":
                        case "AllIn":
                            if (forceNewIfNull && root.UTGRaise == null)
                                root.UTGRaise = new PreflopRangeNode();
                            node = root.UTGRaise;
                            break;
                        default:
                            {
                                if (!action.Contains("bb"))
                                    throw new Exception("Invalid action in range files!");
                                action = action.Replace("bb", "");
                                if (forceNewIfNull && root.UTGRaise == null)
                                    root.UTGRaise = new PreflopRangeNode();
                                if (root.UTGRaise != null)
                                {
                                    root.RaiseVal = float.Parse(action);
                                }
                                node = root.UTGRaise;
                            }
                            break;
                    }
                    break;
                case "MP":
                    switch (action)
                    {
                        case "Call":
                            if (forceNewIfNull && root.MPCall == null)
                                root.MPCall = new PreflopRangeNode();
                            node = root.MPCall;
                            break;
                        case "FOLD":
                            if (forceNewIfNull && root.MPFold == null)
                                root.MPFold = new PreflopRangeNode() { IsFoldNode = true };
                            node = root.MPFold;
                            break;
                        case "Raise":
                        case "AllIn":
                            if (forceNewIfNull && root.MPRaise == null)
                                root.MPRaise = new PreflopRangeNode();
                            node = root.MPRaise;
                            break;
                        default:
                            {
                                if (!action.Contains("bb"))
                                    throw new Exception("Invalid action in range files!");
                                action = action.Replace("bb", "");
                                if (forceNewIfNull && root.MPRaise == null)
                                    root.MPRaise = new PreflopRangeNode();
                                if (root.MPRaise != null)
                                {
                                    root.RaiseVal = float.Parse(action);
                                }
                                node = root.MPRaise;
                            }
                            break;
                    }
                    break;
                case "CO":
                    switch (action)
                    {
                        case "Call":
                            if (forceNewIfNull && root.COCall == null)
                                root.COCall = new PreflopRangeNode();
                            node = root.COCall;
                            break;
                        case "FOLD":
                            if (forceNewIfNull && root.COFold == null)
                                root.COFold = new PreflopRangeNode() { IsFoldNode = true };
                            node = root.COFold;
                            break;
                        case "AllIn":
                        case "Raise":
                            if (forceNewIfNull && root.CORaise == null)
                                root.CORaise = new PreflopRangeNode();
                            node = root.CORaise;
                            break;
                        default:
                            {
                                if (!action.Contains("bb"))
                                    throw new Exception("Invalid action in range files!");
                                action = action.Replace("bb", "");
                                if (forceNewIfNull && root.CORaise == null)
                                    root.CORaise = new PreflopRangeNode();
                                if (root.CORaise != null)
                                {
                                    root.RaiseVal = float.Parse(action);
                                }
                                node = root.CORaise;
                            }
                            break;
                    }
                    break;
                case "BTN":
                    switch (action)
                    {
                        case "Call":
                            if (forceNewIfNull && root.BTNCall == null)
                                root.BTNCall = new PreflopRangeNode();
                            node = root.BTNCall;
                            break;
                        case "FOLD":
                            if (forceNewIfNull && root.BTNFold == null)
                                root.BTNFold = new PreflopRangeNode() { IsFoldNode = true };
                            node = root.BTNFold;
                            break;
                        case "AllIn":
                        case "Raise":
                            if (forceNewIfNull && root.BTNRaise == null)
                                root.BTNRaise = new PreflopRangeNode();
                            node = root.BTNRaise;
                            break;
                        default:
                            {
                                if (!action.Contains("bb"))
                                    throw new Exception("Invalid action in range files!");
                                action = action.Replace("bb", "");
                                if (forceNewIfNull && root.BTNRaise == null)
                                    root.BTNRaise = new PreflopRangeNode();
                                if (root.BTNRaise != null)
                                {
                                    root.RaiseVal = float.Parse(action);
                                }
                                node = root.BTNRaise;
                            }
                            break;
                    }
                    break;
                case "SB":
                    switch (action)
                    {
                        case "Call":
                            if (forceNewIfNull && root.SBCall == null)
                                root.SBCall = new PreflopRangeNode();
                            node = root.SBCall;
                            break;
                        case "FOLD":
                            if (forceNewIfNull && root.SBFold == null)
                                root.SBFold = new PreflopRangeNode() { IsFoldNode = true };
                            node = root.SBFold;
                            break;
                        case "AllIn":
                        case "Raise":
                            if (forceNewIfNull && root.SBRaise == null)
                                root.SBRaise = new PreflopRangeNode();
                            node = root.SBRaise;
                            break;
                        default:
                            {
                                if (!action.Contains("bb"))
                                    throw new Exception("Invalid action in range files!");
                                action = action.Replace("bb", "");
                                if (forceNewIfNull && root.SBRaise == null)
                                    root.SBRaise = new PreflopRangeNode();
                                if (root.SBRaise != null)
                                {
                                    root.RaiseVal = float.Parse(action);
                                }
                                node = root.SBRaise;
                            }
                            break;
                    }
                    break;
                case "BB":
                    switch (action)
                    {
                        case "Call":
                            if (forceNewIfNull && root.BBCall == null)
                                root.BBCall = new PreflopRangeNode();
                            node = root.BBCall;
                            break;
                        case "FOLD":
                            if (forceNewIfNull && root.BBFold == null)
                                root.BBFold = new PreflopRangeNode() { IsFoldNode = true };
                            node = root.BBFold;
                            break;
                        case "AllIn":
                        case "Raise":
                            if (forceNewIfNull && root.BBRaise == null)
                                root.BBRaise = new PreflopRangeNode();
                            node = root.BBRaise;
                            break;
                        default:
                            {
                                if (!action.Contains("bb"))
                                    throw new Exception("Invalid action in range files!");
                                action = action.Replace("bb", "");
                                if (forceNewIfNull && root.BBRaise == null)
                                    root.BBRaise = new PreflopRangeNode();
                                if (root.BBRaise != null)
                                {
                                    root.RaiseVal = float.Parse(action);
                                }
                                node = root.BBRaise;
                            }
                            break;
                    }
                    break;
                default:
                    throw new Exception("Invalid target position in range files!");
            }
            return node;
        }

        private void PopulateFolderRanges(string folder, PreflopRangeNode root)
        {
            foreach (var file in Directory.GetFiles(folder))
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                var currentNode = root;
                var fileSplit = filename.Split("_");

                var splitIndex = 0;
                while (splitIndex < fileSplit.Length - 2)
                {
                    var targetPos = fileSplit[splitIndex];
                    var action = fileSplit[splitIndex + 1];

                    var nextNode = GetNodeFromPositionAction(targetPos, action, currentNode, true);
                    if (nextNode.ParentNode == null)
                    {
                        nextNode.ParentNode = currentNode;
                    }
                    currentNode = nextNode;

                    splitIndex += 2;
                }

                var lastAction = fileSplit[splitIndex + 1];
                if (currentNode.Range == null)
                {
                    currentNode.Range = new Range();
                }

                switch (lastAction)
                {
                    case "Call":
                        currentNode.Range.SetCallFrequency(file);
                        break;
                    case "FOLD":
                        currentNode.Range.SetFoldFrequency(file);
                        break;
                    case "AllIn":
                        currentNode.Range.SetRaiseFrequency(file);
                        break;
                    default:
                        {
                            if (!lastAction.Contains("bb"))
                                throw new Exception("Invalid action in range files!");
                            var raiseVal = lastAction.Replace("bb", "");
                            currentNode.Range.RaiseVal = float.Parse(raiseVal);
                            currentNode.Range.SetRaiseFrequency(file);
                        }
                        break;
                }

            }
        }

    }
}

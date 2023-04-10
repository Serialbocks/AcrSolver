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
                var reverseHand = String.Format("{0}{1}{2}", hand[1], hand[0], hand.Substring(2, hand.Length - 2));
                var frequency = float.Parse(foldSplit[1]);
                _range[hand].FoldFrequency = frequency;
                _range[reverseHand].FoldFrequency = frequency;
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
                var reverseHand = String.Format("{0}{1}{2}", hand[1], hand[0], hand.Substring(2, hand.Length - 2));
                var frequency = float.Parse(callSplit[1]);
                _range[hand].CallFrequency = frequency;
                _range[reverseHand].CallFrequency = frequency;
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
                var reverseHand = String.Format("{0}{1}{2}", hand[1], hand[0], hand.Substring(2, hand.Length - 2));
                var frequency = float.Parse(raiseSplit[1]);
                _range[hand].RaiseFrequency = frequency;
                _range[reverseHand].RaiseFrequency = frequency;
            }
        }
    }

    public class PreflopRangeNode
    {
        public class PositionRanges
        {
            public PreflopRangeNode Raise { get; set; }
            public PreflopRangeNode Call { get; set; }
            public PreflopRangeNode Fold { get; set; }
        }

        public PreflopRangeNode()
        {
            foreach(var position in (Position[])Enum.GetValues(typeof(Position)))
            {
                Children.Add(position, new PositionRanges());
            }
        }

        public Range Range { get; set; }

        public float? RaiseVal { get; set; }
        public bool IsFoldNode { get; set; }

        public Dictionary<Position, PositionRanges> Children { get; set; } = new Dictionary<Position, PositionRanges>();

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
                        currentNode = GetNodeFromPositionAction(bet.Position, "Raise", currentNode);
                        break;
                    case BetType.Call:
                        currentNode = GetNodeFromPositionAction(bet.Position, "Call", currentNode);
                        break;
                    case BetType.Raise:
                        currentNode = GetNodeFromPositionAction(bet.Position, "Raise", currentNode);
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

            foreach(var position in (Position[])Enum.GetValues(typeof(Position)))
            {
                var positionRanges = node.Children[position];
                BringUpFoldRanges(positionRanges.Fold);
                BringUpFoldRanges(positionRanges.Call);
                BringUpFoldRanges(positionRanges.Raise);
            }

            if (!node.IsFoldNode || node.ParentNode == null)
            {
                return;
            }

            var parentNode = node.ParentNode;
            if (parentNode.Range == null)
                parentNode.Range = node.Range;

            foreach (var position in (Position[])Enum.GetValues(typeof(Position)))
            {
                var positionRanges = node.Children[position];
                var parentPositionRanges = parentNode.Children[position];
                if(positionRanges.Fold == null)
                {
                    positionRanges.Fold = parentPositionRanges.Fold;
                }
                if (positionRanges.Call == null)
                {
                    positionRanges.Call = parentPositionRanges.Call;
                }
                if (positionRanges.Raise == null)
                {
                    positionRanges.Raise = parentPositionRanges.Raise;
                }
            }
        }

        private PreflopRangeNode GetNodeOption(PreflopRangeNode root,
            PreflopRangeNode.PositionRanges positionRanges,
            string action,
            bool forceNewIfNull)
        {
            PreflopRangeNode node = null;
            switch (action)
            {
                case "Call":
                    if (forceNewIfNull && positionRanges.Call == null)
                        positionRanges.Call = new PreflopRangeNode();
                    node = positionRanges.Call;
                    break;
                case "FOLD":
                    if (forceNewIfNull && positionRanges.Fold == null)
                        positionRanges.Fold = new PreflopRangeNode() { IsFoldNode = true };
                    node = positionRanges.Fold;
                    break;
                case "Raise":
                case "AllIn":
                    if (forceNewIfNull && positionRanges.Raise == null)
                        positionRanges.Raise = new PreflopRangeNode();
                    node = positionRanges.Raise;
                    break;
                default:
                    {
                        if (!action.Contains("bb"))
                            throw new Exception("Invalid action in range files!");
                        action = action.Replace("bb", "");
                        if (forceNewIfNull && positionRanges.Raise == null)
                            positionRanges.Raise = new PreflopRangeNode();
                        if (positionRanges.Raise != null)
                        {
                            root.RaiseVal = float.Parse(action);
                        }
                        node = positionRanges.Raise;
                    }
                    break;
            }

            return node;
        }
        
        private PreflopRangeNode GetNodeFromPositionAction(Position position, string action, PreflopRangeNode root, bool forceNewIfNull = false)
        {
            var positionRanges = root.Children[position];
            var node = GetNodeOption(root, positionRanges, action, forceNewIfNull);
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
                    Position targetPos;
                    if(!Enum.TryParse(fileSplit[splitIndex], out targetPos))
                    {
                        throw new Exception("Invalid position parsed from range files!");
                    }
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

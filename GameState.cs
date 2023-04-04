using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcrSolver
{
    public class Seat
    {
        public float Bet {
            get
            {
                return _bet;
            }
            set
            {
                if (value != _bet)
                    BetUpdated = true;
                _bet = value;
            }
        }
        public float Stack { get; set; }
        public bool HasCards { get; set; }
        public BoundingBox BetBoundingBox { get; set; }
        public BoundingBox StackBoundingBox { get; set; }
        public Position Position { get; set; }
        public bool BetUpdated { get; set; }

        private float _bet;
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

    public enum BoardState
    {
        Preflop,
        Flop,
        Turn,
        River,
        Error
    }

    public enum Position
    {
        SB,
        BB,
        UTG,
        MP,
        CO,
        BTN
    }

    public enum BetType
    {
        Bet,
        Call,
        Raise
    }

    public class Bet
    {
        public float Amount { get; set; }
        public Position Position { get; set; }
        public BetType Type { get; set; }
        public Seat Seat { get; set; }
        public int PreflopOrder()
        {
            var order = 0;
            switch (Position)
            {
                case Position.BTN:
                    order = 3;
                    break;
                case Position.SB:
                    order = 4;
                    break;
                case Position.BB:
                    order = 5;
                    break;
                case Position.UTG:
                    order = 0;
                    break;
                case Position.MP:
                    order = 1;
                    break;
                case Position.CO:
                    order = 2;
                    break;
                default:
                    break;
            }
            return order;
        }

        public override bool Equals(object betObj)
        {
            var bet = (Bet)betObj;
            return this.Position == bet.Position && this.Amount == bet.Amount;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public static class GameState
    {
        public static List<Seat> Seats { get; set; } = new List<Seat>
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
        public static List<Bet> Bets { get; set; } = new List<Bet>();
        public static Bet CurrentBet { get
            {
                if (Bets.Count == 0)
                    return null;
                return Bets[Bets.Count - 1];
            }
        }
        public static bool AmountsInBB { get; set; }
        public static float Total { get; set; }
        public static int Button { get
            {
                return _button;
            }
            set
            {
                if (value < 0 || _button == value)
                    return;
                _button = value;
                var buttonIndex = Button;
                if (buttonIndex < 0)
                    buttonIndex = 0;

                for(int i = 0; i < Seats.Count; i++)
                {
                    var currentIndex = (i + buttonIndex) % Seats.Count;
                    Seats[currentIndex].Position = (Position)i;
                }
            }
        }
        public static int ActivePlayer { get; set; }
        public static float Pot { get
            {
                var pot = Total;
                foreach(var seat in Seats)
                {
                    pot -= seat.Bet;
                }
                return pot;
            }
        }
        public static List<string> PlayerHand { get; set; } = new List<string>();
        public static List<string> Board
        {
            get
            {
                return _board;
            }
            set
            {
                if(value.Count < _board.Count)
                {
                    _board = value;
                }
                else
                {
                    foreach (var card in value)
                    {
                        if (_board.FirstOrDefault(x => x == card) == null)
                        {
                            _board.Add(card);
                        }
                    }
                }

                ClearCurrentBets();
            }
        }
        public static BoardState BoardState
        {
            get
            {
                switch(Board.Count)
                {
                    case 0:
                        return BoardState.Preflop;
                    case 1:
                    case 2:
                        return BoardState.Error;
                    case 3:
                        return BoardState.Flop;
                    case 4:
                        return BoardState.Turn;
                    case 5:
                        return BoardState.River;
                    default:
                        return BoardState.Error;
                }
            }
        }

        private static List<string> _board { get; set; } = new List<string>();

        private static int _button { get; set; } = -1;

        public static void ClearCurrentBets()
        {
            Bets = new List<Bet>();
        }

        public static Position PlayerPosition()
        {
            return Seats[0].Position;
        }

        public static void UpdateCurrentBets()
        {
            var newBets = new List<Bet>();
            foreach(var seat in Seats)
            {
                if (seat.Bet <= 0 || !seat.BetUpdated)
                    continue;

                newBets.Add(new Bet
                {
                    Amount = seat.Bet,
                    Position = seat.Position,
                    Seat = seat
                });
                seat.BetUpdated = false;
            }

            if (BoardState == BoardState.Preflop)
            {
                // Ensure seat positions are correct
                var currentPosition = Position.SB;
                for (var i = (Button + 1) % Seats.Count; i != Button; i = (i + 1) % Seats.Count)
                {
                    var seat = Seats[i];
                    if (currentPosition == Position.SB && seat.Bet <= 0.5f)
                    {
                        seat.Position = currentPosition;
                        currentPosition = NextPosition(currentPosition);
                    }
                    else if (currentPosition == Position.BB && seat.Bet == 1.0f)
                    {
                        seat.Position = currentPosition;
                        currentPosition = NextPosition(currentPosition);
                    }
                    else if(currentPosition != Position.BB && currentPosition != Position.SB)
                    {
                        seat.Position = currentPosition;
                        currentPosition = NextPosition(currentPosition);
                    }
                }

                newBets = newBets.OrderBy(x => x.PreflopOrder()).OrderBy(x => x.Amount).ToList();
                if (newBets.Count > 0 && newBets[0].Amount <= 0.5f)
                {
                    newBets = newBets.Skip(1).ToList();
                }
                while(newBets.Count > 0 && newBets[0].Amount == 1.0f)
                {
                    newBets = newBets.Skip(1).ToList();
                }
            }
            else
            {
                newBets = newBets.OrderBy(x => x.Position).OrderBy(x => x.Amount).ToList();
            }

            foreach(var bet in newBets)
            {
                if(Bets.Count == 0)
                {
                    bet.Type = BetType.Bet;
                }
                else if(Bets[Bets.Count - 1].Amount == bet.Amount)
                {
                    bet.Type = BetType.Call;
                }
                else
                {
                    bet.Type = BetType.Raise;
                }
                Bets.Add(bet);
            }
        }
        
        public static void ClearSeatBets()
        {
            foreach (var seat in Seats)
            {
                seat.Bet = 0;
                seat.BetUpdated = false;
            }
        }

        public static void ClearPlayerCards()
        {
            foreach(var seat in Seats)
            {
                seat.HasCards = false;
            }
        }

        public static string AmountToString(float amount)
        {
            if(GameState.AmountsInBB)
            {
                return String.Format("{0} BB", amount);
            }
            else
            {
                return amount.ToString("c2");
            }
        }

        public static T NextPosition<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcrSolver
{
    public class Seat
    {
        public float Bet { get; set; }
        public float Stack { get; set; }
        public bool HasCards { get; set; }
        public BoundingBox BetBoundingBox { get; set; }
        public BoundingBox StackBoundingBox { get; set; }
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
        public static bool AmountsInBB { get; set; }
        public static float Total { get; set; }
        public static int Button { get; set; }
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
        public static List<string> Board { get; set; } = new List<string>();

        public static void ClearBets()
        {
            foreach (var seat in Seats)
            {
                seat.Bet = 0;
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
    }
}

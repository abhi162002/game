using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBlackJack.Models
{
    public class Player
    {
        public int Point { get; set; }

        public int AlternatePoint { get; set; }

        public List<Card> CardsAtHand { get; set; } = new List<Card>();

        public int Token { get; set; } = 20;

        public int WinRounds { get; set; }

        public int LostRounds { get; set; }
    }
}

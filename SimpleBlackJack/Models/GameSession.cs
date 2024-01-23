using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBlackJack.Models
{
    public class GameSession
    {
        public Deck Deck { get; set; }

        public Player Player { get; set; }

        public Player Computer { get; set; }

        public bool HasResult { get; set; }

        public string GameStatusMessage { get; set; }

        public int RoundCount { get; set; } = 1;

        public int DrawRounds { get; set; }
    }
}

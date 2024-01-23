using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBlackJack.Models
{
    public class Deck
    {
        private static List<string> SUITS = new List<string>() { "S", "H", "C", "D" };
        private static List<string> VALUES = new List<string>() { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        public List<Card> Cards { get; set; }

        public Deck()
        {
            InitializeDeck();
        }

        public void InitializeDeck()
        {
            Cards = new List<Card>();

            foreach (var value in VALUES)
            {
                foreach (var suit in SUITS)
                {
                    var weight = 0;
                    if (value.Equals("J") || value.Equals("Q") || value.Equals("K"))
                        weight = 10;
                    else if (value.Equals("A"))
                        weight = 11;
                    else
                        weight = Convert.ToInt32(value);

                    Cards.Add(new Card
                    {
                        Value = value,
                        Suit = suit,
                        Weight = weight
                    });
                }
            }
            ShuffleDeck();
        }

        private void ShuffleDeck()
        {
            var j = Cards.Count;
            Random rnd = new Random();
            while (j > 1)
            {
                int i = (rnd.Next(0, j) % j);
                j--;
                var value = Cards[i];
                Cards[i] = Cards[j];
                Cards[j] = value;
            }
        }

        public List<Card> DrawCard(int drawCount)
        {
            var drawnCards = new List<Card>();

            for (int i = 1; i <= drawCount; ++i)
            {
                drawnCards.Add(Cards[i - 1]);
            }
            Cards.RemoveRange(0, drawCount);

            return drawnCards;
        }
    }
}

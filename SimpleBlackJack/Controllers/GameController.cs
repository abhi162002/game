using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleBlackJack.Models;

namespace SimpleBlackJack.Controllers
{
    public class GameController : Controller
    {
        private static string PLAYER_WIN = "YOU WIN!";
        private static string COMPUTER_WIN = "COMPUTER WINS!";
        private static string DRAW = "DRAW";

        public GameSession GameSession { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult Index()
        {
            GameSession = HttpContext.Session.GetObject<GameSession>("gameSession");

            if (GameSession == null)
            {
                InitializeGameSession();
            }

            return View(GameSession);
        }

        public IActionResult NewGame()
        {
            HttpContext.Session.SetObject("gameSession", null);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult AnotherRound()
        {
            GameSession = HttpContext.Session.GetObject<GameSession>("gameSession");

            if (GameSession != null)
            {
                if (GameSession.Player.Token > 0 && GameSession.Computer.Token > 0)
                {
                    GameSession.GameStatusMessage = null;
                    GameSession.Deck.InitializeDeck();

                    GameSession.Player.Point = 0;
                    GameSession.Player.CardsAtHand.Clear();

                    GameSession.Computer.Point = 0;
                    GameSession.Computer.CardsAtHand.Clear();

                    GameSession.HasResult = false;
                    GameSession.GameStatusMessage = null;

                    InitializeCardDraws();
                    GameSession.RoundCount++;
                    Save();
                }
                else
                {
                    ErrorMessage = "Oops... One participant has run out of tokens...";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult DrawCard()
        {
            GameSession = HttpContext.Session.GetObject<GameSession>("gameSession");

            if (GameSession != null && !GameSession.HasResult)
            {
                if (GameSession.Player.CardsAtHand.Count() < 5)
                {
                    GameSession.Player.CardsAtHand.AddRange(GameSession.Deck.DrawCard(1));
                    CalculatePlayerPoint(GameSession.Player);

                    if (GameSession.Player.CardsAtHand.Count() == 5)
                    {
                        var playerLost = IsBusted(GameSession.Player);

                        if (!playerLost) // Charlie Blackjack
                        {
                            PlayerWin(tokenCount: 3);
                        }
                        else // Kaboom
                        {
                            ComputerWin(tokenCount: 3);
                        }
                    }
                    Save();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Stay()
        {
            GameSession = HttpContext.Session.GetObject<GameSession>("gameSession");

            if (GameSession != null && !GameSession.HasResult)
            {
                if (Math.Max(GameSession.Player.Point, GameSession.Player.AlternatePoint) < 16)
                {
                    ErrorMessage = "You do not have enough points!";
                }
                else
                {
                    ComputerTurn();
                    CheckWinner();
                    Save();
                }
            }

            return RedirectToAction(nameof(Index));
        }
        
        #region Game Helper Methods

        private void InitializeGameSession()
        {
            GameSession = new GameSession
            {
                Deck = new Deck(),
                Player = new Player(),
                Computer = new Player()
            };

            InitializeCardDraws();
            Save();
        }

        private void InitializeCardDraws()
        {
            GameSession.Player.CardsAtHand.AddRange(GameSession.Deck.DrawCard(2));
            GameSession.Computer.CardsAtHand.AddRange(GameSession.Deck.DrawCard(2));

            CalculatePlayerPoint(GameSession.Player);
            CalculatePlayerPoint(GameSession.Computer);

            CheckInitialBlackJack();
        }

        private static void CalculatePlayerPoint(Player player)
        {
            var cards = player.CardsAtHand;
            player.Point = 0;
            player.AlternatePoint = 0;

            if (cards.Count() < 3)
            {
                if (cards[0].Value.Equals("A") && cards[1].Value.Equals("A"))
                {
                    player.Point = 21;
                }
                else
                {
                    player.Point = cards[0].Weight + cards[1].Weight;
                }
            }
            else
            {
                var hasAlternateValueCard = cards.Any(card => card.Value.Equals("A"));

                foreach(var card in cards)
                {
                    if (hasAlternateValueCard)
                    {
                        if (card.Value.Equals("A"))
                        {
                            player.Point += 10;
                            player.AlternatePoint += 1;
                        }
                        else
                        {
                            player.Point += card.Weight;
                            player.AlternatePoint += card.Weight;
                        }
                    }
                    else
                    {
                        player.Point += card.Weight;
                    }
                }
            }
        }

        private void CheckInitialBlackJack()
        {
            if (GameSession.Player.Point == 21 && GameSession.Computer.Point == 21)
            {
                DrawGame();
            }
            else if (GameSession.Player.Point == 21)
            {
                PlayerWin(tokenCount: 3);
            }
            else if (GameSession.Computer.Point == 21)
            {
                ComputerWin(tokenCount: 3);
            }
        }

        private void ComputerTurn()
        {
            while (GameSession.Computer.CardsAtHand.Count() < 5 && 
                ((GameSession.Computer.AlternatePoint > 0 && GameSession.Computer.AlternatePoint < 16) || (GameSession.Computer.Point < 16)))
            {
                GameSession.Computer.CardsAtHand.AddRange(GameSession.Deck.DrawCard(1));
                CalculatePlayerPoint(GameSession.Computer);
            }

            if (GameSession.Computer.CardsAtHand.Count() == 4)
            {
                if (GameSession.Computer.AlternatePoint == 16 || GameSession.Computer.Point == 16)
                {
                    GameSession.Computer.CardsAtHand.AddRange(GameSession.Deck.DrawCard(1));
                    CalculatePlayerPoint(GameSession.Computer);
                }
            }
        }

        private void CheckWinner()
        {
            if (GameSession.Computer.CardsAtHand.Count() == 5)
            {
                var computerWin = !IsBusted(GameSession.Computer);

                if (computerWin) // Computer Charlie Blackjack
                {
                    ComputerWin(tokenCount: 3);
                }
                else
                {
                    if (!IsBusted(GameSession.Player))
                    {
                        PlayerWin(tokenCount: 3);
                    }
                    else
                    {
                        DrawGame();
                    }
                }
            }
            else
            {
                if (IsBusted(GameSession.Player) && IsBusted(GameSession.Computer))
                {
                    DrawGame();
                }
                else if (IsBusted(GameSession.Player) && !IsBusted(GameSession.Computer))
                {
                    ComputerWin(tokenCount: 1);
                }
                else if (!IsBusted(GameSession.Player) && IsBusted(GameSession.Computer))
                {
                    PlayerWin(tokenCount: 1);
                }
                else
                {
                    var playerPoint = GameSession.Player.Point;
                    var computerPoint = GameSession.Computer.Point;

                    if (GameSession.Player.AlternatePoint > 15 && 
                        (GameSession.Player.AlternatePoint > playerPoint || playerPoint > 22))
                    {
                        playerPoint = GameSession.Player.AlternatePoint;
                    }

                    if (GameSession.Computer.AlternatePoint > 15 && 
                        (GameSession.Computer.AlternatePoint > computerPoint || computerPoint > 22))
                    {
                        computerPoint = GameSession.Computer.AlternatePoint;
                    }

                    if (playerPoint > computerPoint)
                        PlayerWin(tokenCount: 1);
                    else if (playerPoint < computerPoint)
                        ComputerWin(tokenCount: 1);
                    else
                        DrawGame();
                }
            }
        }

        private bool IsBusted(Player player)
        {
            if (player.AlternatePoint > 15)
            {
                return player.AlternatePoint > 21;
            }
            return (player.Point > 21);
        }

        private void ComputerWin(int tokenCount)
        {
            GameSession.Computer.Token += tokenCount;
            GameSession.Computer.WinRounds++;
            GameSession.Player.Token -= tokenCount;
            GameSession.Player.LostRounds++;

            GameSession.HasResult = true;
            GameSession.GameStatusMessage = COMPUTER_WIN;
        }

        private void PlayerWin(int tokenCount)
        {
            GameSession.Player.Token += tokenCount;
            GameSession.Player.WinRounds++;
            GameSession.Computer.Token -= tokenCount;
            GameSession.Computer.LostRounds++;

            GameSession.HasResult = true;
            GameSession.GameStatusMessage = PLAYER_WIN;
        }

        private void DrawGame()
        {
            GameSession.DrawRounds++;
            GameSession.HasResult = true;
            GameSession.GameStatusMessage = DRAW;
        }

        private void Save()
        {
            HttpContext.Session.SetObject("gameSession", GameSession);
        }

        #endregion
    }
}

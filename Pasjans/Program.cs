using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Pasjans
{
    internal class Program
    {
        static void Validate(Game game)
        {
            List<Card> seen = new List<Card>();
            int colIndex = 0;
            foreach (Column column in game.Columns)
            {
                Card previousCard = null;
                foreach (Card card in column.Cards)
                {
                    if (previousCard != null)
                    {
                        if (!(card.Color != previousCard.Color && Array.IndexOf(Game.RANKS, previousCard.Rank) + 1 == Array.IndexOf(Game.RANKS, card.Rank)) && !card.Covered && !previousCard.Covered || seen.Contains(card)) 
                        { 
                            throw new NotImplementedException($"Walidacja zakończona niepomyślnie. Kolumna {colIndex+1}, karty {previousCard} oraz {card}");
                        }
                    }
                    previousCard = card;
                    seen.Add(card);
                }
                colIndex++;
            }
            Console.WriteLine("Walidacja zakończona pomyślnie.");
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Pasjans";

            Game game = new Game();
            while (true)
            {
                game.Display();
                game.Interact();
                Validate(game);
                Console.Write("\n\n");
                
                //break;
            }
        }
    }
}

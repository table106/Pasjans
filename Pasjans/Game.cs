using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;

namespace Pasjans
{
    /// <summary>
    /// Kontener na operacje pojedynczej gry
    /// </summary>
    internal class Game
    {
        public List<Column> Columns { get; }
        private List<Card> Deck;
        private readonly Dictionary<string, List<Card>> Stack;
        private Card DeckCard;
        private List<Card> SpentDeck;
        private static readonly Random random = new Random();
        private static readonly string[] RANKS = { "K", "Q", "J", "10", "9", "8", "7", "6", "5", "4", "3", "2", "A" };
        private static readonly string[] SUITS = { "spades", "hearts", "diamonds", "clubs" };

        public Game()
        {
            // create deck
            Deck = new List<Card>();
            foreach (string suit in SUITS)
            {
                foreach (string rank in RANKS)
                {
                    Deck.Add(new Card(rank, suit));
                }
            }
            ShuffleDeck();

            // create finish stack
            Stack = new Dictionary<string, List<Card>>();
            foreach (string suit in SUITS)
            {
                Stack.Add(suit, new List<Card>());
            }

            // define deck card and spent deck
            DeckCard = null;
            SpentDeck = new List<Card>();

            // create columns
            Columns = new List<Column>();
            for (int i = 0; i < 7; i++)
            {
                Columns.Add(new Column());
                for (int j = 0; j <= i; j++)
                {
                    Card card = Deck[0];
                    Deck.RemoveAt(0);
                    if (j == i)
                    {
                        card.Covered = false;
                    }
                    Columns[i].Cards.Add(card);
                }
            }
        }

        /// <summary>
        /// Miesza zawartość talii
        /// </summary>
        private void ShuffleDeck()
        {
            // fisher-yates
            for (int i = Deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (Deck[j], Deck[i]) = (Deck[i], Deck[j]);
            }
        }

        /// <summary>
        /// Odkrywa nową kartę z talii, poprzednią kładzie na stos zużytych. Nie używać gdy <c>DeckCard</c> to <c>null</c>!
        /// </summary>
        private void NextDeckCard()
        {
            if (DeckCard != null)
            {
                DeckCard.Covered = true;
                SpentDeck.Add(DeckCard);
            }
            DeckCard = Deck[0];
            DeckCard.Covered = false;
            Deck.RemoveAt(0);
        }

        /// <summary>
        /// Sprawdza, czy ruch jest dozwolony
        /// </summary>
        /// <param name="target">Karta ruszana</param>
        /// <param name="dest">Kolumna na którą kładziemy <c>target</c></param>
        /// <returns><c>bool</c>: Poprawność ruchu</returns>
        private bool ValidMovement(Card target, Column dest)
        {
            Card last = dest[dest.Cards.Count - 1];
            if (dest.Cards.Count == 0)
            {
                return target.Rank == "K";
            }
            else if (last.Rank == "2")
            {
                return false;
            }
            return target.Color != last.Color && Array.IndexOf(RANKS, last.Rank) + 1 == Array.IndexOf(RANKS, target.Rank);
        }
        /// <summary>
        /// Sprawdza, czy ruch jest dozwolony
        /// </summary>
        /// <param name="target">Karta ruszana</param>
        /// <param name="dest">Stos końcowy na który kładziemy</param>
        /// <returns><c>bool</c>: Poprawność ruchu</returns>
        private bool ValidMovement(Card target, List<Card> dest)
        {
            if (dest.Count == 0)
            {
                return target.Rank == "A";
            }
            return Array.IndexOf(RANKS, dest[dest.Count-1].Rank) + 1 == Array.IndexOf(RANKS, target.Rank);
        }

        /// <summary>
        /// Wyświetla stan kolumn, talii, odkrytej karty oraz stosów końcowych
        /// </summary>
        public void Display()
        {
            StringBuilder ret = new StringBuilder();

            // odkryta
            if (DeckCard != null)
            {
                ret.Append(DeckCard.ToString()+"   ");
            }
            else
            {
                ret.Append("---   ");
            }

            // talia
            if (Deck.Count != 0)
            {
                ret.Append("---\t     ");
            }
            else
            {
                ret.Append("               ");
            }
            
            // stosy końcowe
            foreach (var stack in Stack)
            {
                int topCardIndex = stack.Value.Count - 1;
                if (topCardIndex < 0) { ret.Append("---   "); }
                else { ret.Append(stack.Value[topCardIndex].ToString() + "   "); }
            }
            ret.Length--;
            ret.AppendLine();

            // numery na górze
            ret.Append("   ");
            for (int x = 1; x <= 7; x++)
            {
                ret.Append($" {x}    ");
            }
            ret.AppendLine();

            // cards
            for (int x = 0; x < Columns.Where(col => col != null && col.Cards != null)
                .Select(col => col.Cards.Count)
                .DefaultIfEmpty(0).Max(); x++)
            {
                ret.Append($"{x + 1}. ");
                for (int y = 0; y < 7; y++)
                {
                    Column current = Columns[y];
                    if (current.Cards.Count > x)
                    {
                        Card card = current[x];
                        ret.Append(card);
                    }
                    else
                    {
                        ret.Append("   ");
                    }
                    ret.Append("   ");
                }
                ret.AppendLine();
            }
            Console.Write(ret.ToString());
        }

        /// <summary>
        /// Wchodzi w interakcję z użytkownikiem
        /// </summary>
        public void Interact()
        {
            Console.WriteLine("Jaki chcesz wykonać ruch?");
            Console.WriteLine("[S] - Przełożyć karty ze stołu");
            Console.WriteLine("[T] - Przełożyć kartę z talii");
            Console.WriteLine("[N] - " + (Deck.Count != 0 ? "Odkryć kartę z talii" : "Przetasować talię"));
            Console.WriteLine("[F] - Przełożyć kartę ze stosu końcowego");
            string choice = Console.ReadLine();
            choice = choice.ToUpper();
            switch (choice)
            {
                case "S":
                    {
                        Console.WriteLine("Wybierz kolumnę");
                        int col = int.Parse(Console.ReadLine());
                        Console.WriteLine("Wybierz kartę");
                        int i = 1;
                        foreach(Card card in Columns[col-1].Cards)
                        {
                            i++;
                            if (card.Covered == true) { continue; }
                            Console.WriteLine($"{i}. "+card);
                        }
                        int cardChoice = int.Parse(Console.ReadLine());
                        Console.WriteLine("Przenieś do innej kolumny");
                        // Columns[col-1].Cards[card-1]
                        break;
                    }
                case "T":
                    {
                        Console.WriteLine("Chcę położyć kartę na\n[S] - stosie końcowym\n[K] - kolumnie");
                        choice = Console.ReadLine();
                        choice = choice.ToUpper();
                        if (choice == "S")
                        {
                            List<Card> dest = Stack[DeckCard.Suit];
                            if (ValidMovement(DeckCard, dest))
                            {
                                dest.Add(DeckCard);
                                DeckCard = null;
                            }
                            else
                            {
                                Console.WriteLine("Ten ruch nie jest dozwolony.");
                            }
                        } 
                        else if (choice == "K")
                        {
                            Console.WriteLine($"Na której kolumnie chcesz położyć {DeckCard}?");
                            choice = Console.ReadLine();
                            int colNum = int.Parse(choice);
                            if (colNum < 1 || colNum > 7) { Console.WriteLine("Ta kolumna nie istnieje."); }
                            if (ValidMovement(DeckCard, Columns[colNum - 1]))
                            {
                                Columns[colNum - 1].Cards.Add(DeckCard);
                                DeckCard = null;
                            }
                            else
                            {
                                Console.WriteLine("Ten ruch nie jest dozwolony.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Niepoprawny wybór.");
                        }
                        break;
                    }
                case "N":
                    {
                        if (Deck.Count == 0)
                        {
                            DeckCard.Covered = true;
                            SpentDeck.Add(DeckCard);
                            DeckCard = null;
                            Deck = SpentDeck;
                            SpentDeck = new List<Card>();
                            ShuffleDeck();
                        }
                        else if (DeckCard == null)
                        {
                            NextDeckCard();
                        }
                        else
                        {
                            DeckCard.Covered = true;
                            SpentDeck.Add(DeckCard);
                            NextDeckCard();
                        }
                        break;
                    }
                case "F":
                    {
                        // move
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Niepoprawny wybór.");
                        break;
                    }
            }
        }
    }
}

using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, List<Card>> FinalStacks;
        private Card RevealedCard;
        private readonly List<Card> SpentDeck;
        private static readonly Random random = new Random();
        public static readonly string[] RANKS = { "K", "Q", "J", "10", "9", "8", "7", "6", "5", "4", "3", "2", "A" };
        public static readonly string[] SUITS = { "spades", "hearts", "diamonds", "clubs" };

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
            FinalStacks = new Dictionary<string, List<Card>>();
            foreach (string suit in SUITS)
            {
                FinalStacks.Add(suit, new List<Card>());
            }

            // define deck card and spent deck
            RevealedCard = null;
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
                    if (j == i) card.Covered = false;
                    Columns[i].Cards.Add(card);
                }
            }
        }

        /// <summary>
        /// Wyświetla błąd
        /// </summary>
        /// <param name="msg"><c>string</c>: Wiadomość do przekazania</param>
        private void Error(string msg)
        {
            Console.WriteLine(msg);
            Console.WriteLine("Naciśnij dowolny klawisz by kontynuować...");
            Console.ReadKey();
        }

        /// <summary>
        /// Określa, czy gra dobiegła końca
        /// </summary>
        /// <returns><c>bool</c>: czy nastąpił koniec gry</returns>
        public bool Finished()
        {
            foreach (var stackPair in FinalStacks)
            {
                List<Card> expect = new List<Card>();
                foreach (string rank in RANKS.Reverse().ToArray())
                {
                    expect.Add(new Card(rank, stackPair.Key));
                }
                if (expect != stackPair.Value) return false;
            }
            return true;
        }

        /// <summary>
        /// Miesza zawartość talii algorytmem Fisher-Yates
        /// </summary>
        private void ShuffleDeck()
        {
            for (int i = Deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (Deck[j], Deck[i]) = (Deck[i], Deck[j]);
            }
        }

        /// <summary>
        /// Sprawdza, czy ruch jest dozwolony
        /// </summary>
        /// <param name="target">Karta ruszana</param>
        /// <param name="dest">Kolumna na którą kładziemy <c>target</c></param>
        /// <returns><c>bool</c>: Poprawność ruchu</returns>
        private bool ValidMovement(Card target, Column dest)
        {
            if (dest.Cards.Count == 0) return target.Rank == "K";
            Card last = dest.Last();
            if (last.Rank == "2") return false;
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
            if (dest.Count == 0) return target.Rank == "A";
            return Array.IndexOf(RANKS, dest[dest.Count - 1].Rank) - 1 == Array.IndexOf(RANKS, target.Rank);
        }

        /// <summary>
        /// Wyświetla stan kolumn, talii, odkrytej karty oraz stosów końcowych
        /// </summary>
        public void Display()
        {
            StringBuilder ret = new StringBuilder();

            // odkryta
            if (RevealedCard != null) ret.Append(RevealedCard.ToString() + "   ");
            else ret.Append("      ");

            // talia
            if (Deck.Count != 0) ret.Append("---\t     ");
            else ret.Append("               ");

            // stosy końcowe
            foreach (var stack in FinalStacks)
            {
                int topCardIndex = stack.Value.Count - 1;
                if (topCardIndex < 0 || stack.Value[topCardIndex] == null) ret.Append("---   ");
                else { ret.Append(stack.Value[topCardIndex].ToString() + "   "); }
            }
            ret.Length--;
            ret.AppendLine();
            ret.AppendLine();

            // numery na górze
            ret.Append("   ");
            for (int x = 1; x <= 7; x++) ret.Append($" {x}    ");
            ret.AppendLine();

            // cards
            for (int x = 0; x < Columns.Where(col => col != null && col.Cards != null)
                .Select(col => col.Cards.Count)
                .DefaultIfEmpty(0).Max(); x++)
            {
                ret.Append($"{x + 1}." + (x+1 < 10 ? " " : ""));
                for (int y = 0; y < 7; y++)
                {
                    Column current = Columns[y];
                    if (current.Cards.Count > x)
                    {
                        Card card = current[x];
                        ret.Append(card);
                    }
                    else ret.Append("   ");
                    ret.Append("   ");
                }
                ret.AppendLine();
            }
            Console.Write(ret.ToString());
        }

        /// <summary>
        /// Wchodzi w interakcję z użytkownikiem
        /// </summary>
        public void Interact(out bool userEnd)
        {
            userEnd = false;
            Console.WriteLine("Co chcesz zrobić?");
            Console.WriteLine("[S] - Przełożyć karty ze stołu");
            Console.WriteLine("[T] - Przełożyć odkrytą kartę z talii");
            Console.WriteLine("[N] - " + (Deck.Count != 0 ? "Odkryć kartę z talii" : "Przetasować talię"));
            Console.WriteLine("[K] - Przełożyć kartę ze stosu końcowego");
            Console.WriteLine("[W] - Zakończyć tą rozgrywkę");
            Console.Write(">");
            string choice = Console.ReadLine().ToUpper();
            switch (choice)
            {
                case "S":
                    {
                        Console.WriteLine("Wybierz kolumnę");
                        Console.Write(">");
                        if (!int.TryParse(Console.ReadLine(), out int targetColIndex))
                        {
                            Error("Wprowadzono niepoprawną wartość.");
                            break;
                        }
                        if (targetColIndex > 7 || targetColIndex < 1)
                        {
                            Error("Ta kolumna nie istnieje.");
                            break;
                        }
                        Column targetCol = Columns[targetColIndex-1];
                        Console.WriteLine("Wybierz kartę");
                        int i = 0;
                        foreach (Card card in targetCol)
                        {
                            i++;
                            if (card.Covered == true) continue;
                            Console.WriteLine($"{i}. " + card);
                        }
                        Console.Write(">");
                        if (!int.TryParse(Console.ReadLine(), out int cardChoiceIndex))
                        {
                            Error("Wprowadzono niepoprawną wartość.");
                            break;
                        }
                        if (cardChoiceIndex > targetCol.Cards.Count || cardChoiceIndex < 0)
                        {
                            Error("Ta karta nie istnieje.");
                            break;
                        }
                        Card chosenCard = targetCol[cardChoiceIndex-1];
                        if (chosenCard.Covered)
                        {
                            Error("Wybrana karta jest zakryta.");
                            break;
                        }
                        Console.WriteLine("Przenieś na\n[S] - stół\n[K] - stos końcowy");
                        Console.Write(">");
                        choice = Console.ReadLine().ToUpper();
                        if (choice == "S")
                        {
                            Console.WriteLine("Przenieść do której kolumny?");
                            Console.Write(">");
                            if (!int.TryParse(Console.ReadLine(), out int destColIndex))
                            {
                                Error("Wprowadzono niepoprawną wartość.");
                                break;
                            }
                            if (destColIndex > 7 || destColIndex < 1)
                            {
                                Error("Ta kolumna nie istnieje.");
                                break;
                            }
                            Column destCol = Columns[destColIndex - 1];
                            if (ValidMovement(chosenCard, destCol))
                            {
                                int countToMove = targetCol.Cards.Count - cardChoiceIndex + 1;
                                List<Card> cardsToMove = targetCol.Cards.GetRange(cardChoiceIndex - 1, countToMove);
                                targetCol.Cards.RemoveRange(cardChoiceIndex - 1, countToMove);
                                destCol.Cards.AddRange(cardsToMove);

                                if (targetCol.Cards.Count > 0) targetCol.Last().Covered = false;
                            }
                            else Error("Ten ruch nie jest dozwolony.");
                        }
                        else if (choice == "K")
                        {
                            List<Card> dest = FinalStacks[chosenCard.Suit];
                            if (!(Array.IndexOf(targetCol.ToArray(), chosenCard) == targetCol.Cards.Count - 1))
                            {
                                Error("Jedynie pojedyncze karty można przekładać na stos końcowy.");
                                break;
                            }
                            if (ValidMovement(chosenCard, dest))
                            {
                                dest.Add(chosenCard);
                                targetCol.Cards.Remove(chosenCard);
                                if (targetCol.Cards.Count > 0) targetCol.Last().Covered = false;
                            }
                            else Error("Ten ruch nie jest dozwolony.");
                        }
                        else Error("Niepoprawny wybór.");
                        break;
                    }
                case "T":
                    {
                        if (RevealedCard == null) 
                        { 
                            Error("Nie odkryto żadnej karty z talii."); 
                            break;
                        }
                        Console.WriteLine("Przełóż kartę na\n[S] - stół\n[K] - stos końcowy");
                        Console.Write(">");
                        choice = Console.ReadLine().ToUpper();
                        if (choice == "S")
                        {
                            Console.WriteLine($"Na której kolumnie położyć {RevealedCard}?");
                            Console.Write(">");
                            if (!int.TryParse(Console.ReadLine(), out int colNum))
                            {
                                Error("Wprowadzono niepoprawną wartość.");
                                break;
                            }
                            if (colNum < 1 || colNum > 7)
                            { 
                                Error("Ta kolumna nie istnieje.");
                                break; 
                            }
                            if (ValidMovement(RevealedCard, Columns[colNum - 1]))
                            {
                                Columns[colNum - 1].Cards.Add(new Card(RevealedCard));
                                RevealedCard = null;
                            }
                            else Error("Ten ruch nie jest dozwolony.");
                        }
                        else if (choice == "K")
                        {
                            List<Card> dest = FinalStacks[RevealedCard.Suit];
                            if (ValidMovement(RevealedCard, dest))
                            {
                                dest.Add(new Card(RevealedCard));
                                RevealedCard = null;
                            }
                            else Error("Ten ruch nie jest dozwolony.");
                        }
                        else Error("Niepoprawny wybór.");
                        break;
                    }
                case "N":
                    {
                        if (RevealedCard != null)
                        {
                            SpentDeck.Add(new Card(RevealedCard));
                            RevealedCard = null;
                        }
                        if (Deck.Count == 0)
                        {
                            Deck = SpentDeck.ToList();
                            SpentDeck.Clear();
                            ShuffleDeck();
                        }
                        else
                        {
                            RevealedCard = Deck[0];
                            Deck.RemoveAt(0);
                            RevealedCard.Covered = false;
                        }
                        break;
                    }
                case "K":
                    {
                        Console.WriteLine("Wybierz kartę");
                        Console.Write(">");
                        foreach (var stackPair in FinalStacks)
                        {
                            if (stackPair.Value.Count > 0) Console.WriteLine($"{stackPair.Key} - {stackPair.Value.Last()}");
                        }
                        choice = Console.ReadLine();
                        if (!SUITS.Contains(choice))
                        {
                            Error("Wprowadzono niepoprawną wartość.");
                            break;
                        }
                        Card targetCard = FinalStacks[choice].Last();
                        Console.WriteLine("Przenieść do której kolumny?");
                        Console.Write(">");
                        if (!int.TryParse(Console.ReadLine(), out int colNum))
                        {
                            Error("Wprowadzono niepoprawną wartość.");
                            break;
                        }
                        if (colNum < 1 || colNum > 7)
                        {
                            Error("Ta kolumna nie istnieje.");
                            break;
                        }
                        if (ValidMovement(targetCard, Columns[colNum - 1]))
                        {
                            Columns[colNum - 1].Cards.Add(new Card(targetCard));
                        }
                        else Error("Ten ruch nie jest dozwolony.");
                        break;
                    }
                case "W":
                    {
                        userEnd = true;
                        return;
                    }
                default:
                    {
                        Error("Niepoprawny wybór.");
                        break;
                    }
            }
        }
    }
}

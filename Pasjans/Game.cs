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
        private List<Column> Columns { get; }
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
        /// <param name="msg">Wiadomość do przekazania</param>
        private void Error(string msg)
        {
            Console.WriteLine(msg);
            Console.WriteLine("Naciśnij dowolny klawisz by kontynuować...");
            Console.ReadKey();
        }

        /// <summary>
        /// Określa, czy gra dobiegła końca
        /// </summary>
        /// <returns>Czy nastąpił koniec gry</returns>
        public bool Finished()
        {
            foreach (var pair in FinalStacks)
            {
                if (pair.Value.Count != 13)
                {
                    return false;
                }
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
        /// <returns>Poprawność ruchu</returns>
        private bool ValidMovement(Card target, Column dest)
        {
            if (dest.Cards.Count == 0) return target.Rank == "K";
            Card last = dest.Last();
            if (last.Rank == "A") return false;
            return target.Color != last.Color && Array.IndexOf(RANKS, last.Rank) + 1 == Array.IndexOf(RANKS, target.Rank);
        }
        /// <summary>
        /// Sprawdza, czy ruch jest dozwolony
        /// </summary>
        /// <param name="target">Karta ruszana</param>
        /// <param name="dest">Stos końcowy na który kładziemy</param>
        /// <returns>Poprawność ruchu</returns>
        private bool ValidMovement(Card target, List<Card> dest)
        {
            if (dest.Count == 0) return target.Rank == "A";
            return Array.IndexOf(RANKS, dest[dest.Count - 1].Rank) - 1 == Array.IndexOf(RANKS, target.Rank);
        }

        /// <summary>
        /// Wprowadza oraz waliduje liczbę od użytkownika.
        /// </summary>
        /// <param name="prompt">Zapytanie do użytkownika</param>
        /// <param name="min">Minimalna wartość wymagana</param>
        /// <param name="max">Maksymalna wartość wymagana</param>
        /// <param name="errorMessage">Wiadomość błędu do wyświetlenia</param>
        /// <param name="result">Wprowadzona liczba</param>
        /// <returns>Poprawność wprowadzonych danych</returns>
        private bool GetIntInput(string prompt, int min, int max, string errorMessage, out int result)
        {
            Console.WriteLine(prompt);
            Console.Write(">");
            if (int.TryParse(Console.ReadLine(), out result))
            {
                if (result >= min && result <= max)
                {
                    return true;
                }
                else
                {
                    Error(errorMessage);
                    return false;
                }
            }
            else
            {
                Error("Wprowadzono niepoprawną wartość.");
                return false;
            }
        }

        /// <summary>
        /// Wyświetla stan kolumn, talii, odkrytej karty oraz stosów końcowych
        /// </summary>
        public void Display()
        {
            /// <summary>Ustawia kolor tekstu konsoli na podstawie karty</summary>
            /// <param name="card">Karta, dla której ustawiany jest kolor</param>
            void SetCardColor(Card card)
            {
                if (card.Color == Color.Red) Console.ForegroundColor = ConsoleColor.Red;
                else Console.ForegroundColor = ConsoleColor.White;
            }

            // odkryta karta z talii
            if (RevealedCard != null)
            {
                SetCardColor(RevealedCard);
                Console.Write(RevealedCard.ToString());
                Console.Write("    ");
            }
            else
            {
                Console.Write("       ");
            }
            Console.ForegroundColor = ConsoleColor.White;

            // talia
            if (Deck.Count != 0)
            {
                Console.Write("---\t        "); 
            }
            else
            {
                Console.Write("                 ");
            }

            // stosy końcowe
            foreach (KeyValuePair<string, List<Card>> stack in FinalStacks)
            {
                int topCardIndex = stack.Value.Count - 1;
                if (topCardIndex < 0 || stack.Value[topCardIndex] == null)
                {
                    Console.Write("---    ");
                }
                else
                {
                    Card topCard = stack.Value[topCardIndex];
                    SetCardColor(topCard);
                    Console.Write(topCard.ToString());
                    Console.Write("    ");
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine();
            Console.WriteLine();

            // numery kolumn
            Console.Write("   ");
            for (int x = 1; x <= 7; x++)
            {
                Console.Write($" {x}     ");
            }
            Console.WriteLine();

            // karty
            for (int x = 0; x < Columns.Select(col => col.Cards.Count).DefaultIfEmpty(0).Max(); x++)
            {
                Console.Write($"{x + 1}." + (x + 1 < 10 ? " " : ""));

                for (int y = 0; y < 7; y++)
                {
                    Column current = Columns[y];
                    if (current.Cards.Count > x)
                    {
                        Card card = current[x];
                        if (!card.Covered) SetCardColor(card);
                        Console.Write(card.ToString());
                    }
                    else
                    {
                        Console.Write("   ");
                    }
                    Console.Write("    ");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.White;
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
                        if (!GetIntInput("Wybierz kolumnę", 1, 7, "Ta kolumna nie istnieje.", out int targetColIndex)) break;
                        Column targetCol = Columns[targetColIndex-1];
                        for (int i = 0; i < targetCol.Cards.Count; i++)
                        {
                            Card card = targetCol.Cards[i];
                            if (card.Covered) continue;
                            Console.WriteLine($"{i+1}. " + card);
                        }
                        if (!GetIntInput("Wybierz kartę", 1, targetCol.Cards.Count, "Ta karta nie istnieje.", out int cardChoiceIndex)) break;
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
                            if (!GetIntInput("Przenieść do której kolumny?", 1, 7, "Ta kolumna nie istnieje.", out int destColIndex)) break;
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
                            if (!GetIntInput($"Na której kolumnie położyc {RevealedCard}?", 1, 7, "Ta kolumna nie istnieje.", out int colNum)) break;
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
                        if (!Array.TrueForAll(FinalStacks.Values.ToArray(), x => x.Count > 0))
                        {
                            Error("Żadna karta nie jest na stosie końcowym.");
                            break;
                        }
                        Console.WriteLine("Wybierz kartę");
                        Console.Write(">");
                        foreach (KeyValuePair<string, List<Card>> stackPair in FinalStacks)
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
                        if (!GetIntInput("Przenieść do której kolumny?", 1, 7, "Ta kolumna nie istnieje.", out int colNum)) break;
                        if (ValidMovement(targetCard, Columns[colNum - 1]))
                        {
                            Columns[colNum - 1].Cards.Add(new Card(targetCard));
                            FinalStacks[choice].Remove(targetCard);
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

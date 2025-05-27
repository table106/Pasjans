namespace Pasjans
{
    /// <summary>
    /// Kontener na istniejące kolory
    /// </summary>
    public enum Color
    {
        Red,
        Black
    }

    /// <summary>
    /// Karta do gry
    /// </summary>
    internal class Card
    {
        public string Rank { get; }
        public Color Color { get; }
        public string Suit { get; }
        public bool Covered { get; set; }
        private readonly char Icon;

        public Card(string rank, string suit)
        {
            Rank = rank;
            Suit = suit;
            Covered = true;
            switch (suit)
            {
                case "clubs":
                    {
                        Icon = '♣';
                        Color = Color.Black;
                        break;
                    }
                case "spades":
                    {
                        Icon = '♠';
                        Color = Color.Black;
                        break;
                    }
                case "hearts":
                    {
                        Icon = '♥';
                        Color = Color.Red;
                        break;
                    }
                case "diamonds":
                    {
                        Icon = '♦';
                        Color = Color.Red;
                        break;
                    }
            }
        }
        public Card(Card original)
        {
            Rank = original.Rank;
            Suit = original.Suit;
            Covered = original.Covered;
            Color = original.Color;
            Icon = original.Icon;
        }

        public override string ToString()
        {
            if (Covered) return "---";
            else if (Rank == "10") return $"10{Icon}";
            return $"{Rank} {Icon}";
        }
    }
}

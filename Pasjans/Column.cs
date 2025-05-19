using System.Collections.Generic;
using System.Text;

namespace Pasjans
{
    /// <summary>
    /// Pojedyncza kolumna w rozgrywce
    /// </summary>
    internal class Column
    {
        public List<Card> Cards { get; }

        public Column()
        {
            Cards = new List<Card>();
        }

        public Card this[int i]
        {
            get { return Cards[i]; }
            set { Cards[i] = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Card card in Cards)
            {
                sb.AppendLine(card.ToString());
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}

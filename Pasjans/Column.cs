using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Pasjans
{
    /// <summary>
    /// Pojedyncza kolumna w rozgrywce
    /// </summary>
    internal class Column : IEnumerable<Card>
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

        public IEnumerator<Card> GetEnumerator()
        {
            return Cards.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

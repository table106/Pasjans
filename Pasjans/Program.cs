using System;
using System.Text;

namespace Pasjans
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Pasjans";

            Game game = new Game();
            while (true)
            {
                game.Display();
                game.Interact();
                Console.WriteLine();
                
                //break;
            }
        }
    }
}

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
                game.Interact(out bool userEnd);
                if (game.Finished() || userEnd) break;
                Console.Clear();
            }
            Console.Clear();
            Console.WriteLine("Chcesz zagrać jeszcze raz?\n[T] - Tak\n[N] - Nie");
            Console.Write(">");
            if (Console.ReadLine().ToUpper() == "T") 
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                Main(args);
            }
        }
    }
}

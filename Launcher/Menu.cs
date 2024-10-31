using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    internal static class Menu
    {
        public static void WriteMainMenu()
        {
            Console.WriteLine("ONEXMM - Launcher Menu");
            Console.WriteLine("----------------------" + Environment.NewLine);
            Console.WriteLine("Select an action (validate with ENTER):");
            Console.WriteLine("1. Set pairs for fixing process");
            Console.WriteLine("2. Check pairs under management");
            Console.WriteLine("3. Launch fixing process");
            Console.WriteLine("4. Stop fixing process");
            Console.WriteLine("_______________________________________");
            Console.WriteLine("0. Exit");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("-> Choice");
        }

        public static void ShowPairsInScope()
        {
            Console.WriteLine();
            Console.WriteLine("Pairs in scope:");
            Console.WriteLine("---------------");

            foreach (var pair in Tempos.PairsInScope)
                Console.WriteLine($" {pair.Symbol}");

            Console.WriteLine();
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }
}

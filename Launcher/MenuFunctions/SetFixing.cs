using OneXMM;
using OneXMM.Utils;
using System;

namespace Launcher.MenuFunctions
{
    internal static class SetFixing
    {
        public static void LaunchFixingMenu()
        {
            Console.Clear();
            Console.WriteLine("Set pairs for fixing process");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Select the pairs (use ; as pair seperator):");

            var count = 1;
            var pairs = Config.GetPairs();

            foreach (var pair in pairs)
            {
                Console.WriteLine($"{count}. {pair.Symbol}_{pair.Chain}");
                count++;
            }
            Console.WriteLine("_______________________________________");
            Console.WriteLine("0. Return");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("-> Choice");

            ConsoleKeyInfo key;
            var cmd = "";

            do
            {
                key = Console.ReadKey(true);
                
                if (key.KeyChar == '0') return;
                if ((int)key.KeyChar >= 48 && (int)key.KeyChar <= 59)
                {
                    Console.Write(key.KeyChar);
                    cmd += key.KeyChar;
                }
                else if (key.KeyChar == ';')
                {
                    Console.Write(key.KeyChar);
                    cmd += ';';
                }
            } while (key.Key != ConsoleKey.Enter);

            cmd = cmd.TrimEnd(';');
            var pairIds = cmd.Split(';');

            foreach (var pairId in pairIds)
            {
                var pair = pairs[Convert.ToUInt32(pairId) - 1];
                Tempos.AddPairToTempos(pair, Config.GetFixingIntervalsForPair(pair).Merge());
            }
        }
    }
}

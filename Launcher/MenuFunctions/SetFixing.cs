using System;

namespace Launcher.MenuFunctions
{
    using Launcher.Configuration;
    using OneXMM;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    internal static class SetFixing
    {
        internal static Pair[] _selectedPairs;

        public static void LaunchFixingMenu()
        {
            Console.Clear();
            Console.WriteLine("Set pairs for fixing process");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Select the pairs (use ; as pair seperator):");
            Console.WriteLine("999. Add pre-set list");

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

            var selectedPairs = new List<Pair>(20);

            foreach (var pairId in pairIds)
            {
                if (pairId.Equals("999"))
                {
                    var filePath = Path.Combine(AppContext.BaseDirectory, "Configuration\\DefaultPairs.json");
                    var preset = Utils.ConvertFileToObject<DefaultPairs>(filePath) ?? throw new Exception("SetFixing - Cannot read DefaultPairs.json.");
                    var listOfPresetPairs = new List<Pair>(preset.Pairs.Length);

                    foreach (var pairSetting in preset.Pairs)
                    {
                        listOfPresetPairs.Add(Config.GetPair(pairSetting.Symbol, pairSetting.Chain));
                        throw new Exception();
                    }

                    if (pairIds.Length == 1)
                    {
                        
                    }
                    else
                    {

                    }
                }
                else
                {
                    selectedPairs.Add(pairs[Convert.ToUInt32(pairId) - 1]);
                }
            }

            _selectedPairs = selectedPairs.ToArray();

            KycChoice.LaunchKycChoiceMenu();
        }
    }
}

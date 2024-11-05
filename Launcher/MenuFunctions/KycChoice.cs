using System;

namespace Launcher.MenuFunctions
{
    using OneXMM;
    using OneXMM.Utils;

    internal static class KycChoice
    {
        public static void LaunchKycChoiceMenu()
        {
            Console.Clear();
            Console.WriteLine("Set the KYC status for pairs");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Select the KYC status to be managed:");
            Console.WriteLine("1. All");
            Console.WriteLine("2. Non-KYC");
            Console.WriteLine("3. KYC");
            Console.WriteLine("_______________________________________");
            Console.WriteLine("0. Return");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("-> Choice");

            ConsoleKeyInfo key;
            char cmd = (char)0;

            do
            {
                key = Console.ReadKey(true);

                if (key.KeyChar == '0') return;
                
                if ((int)key.KeyChar >= 48 && (int)key.KeyChar <= 51)
                {
                    if ((int)cmd != 0) { Console.CursorLeft = 0; }

                    Console.Write(key.KeyChar);
                    cmd = key.KeyChar;
                }
            } while (key.Key != ConsoleKey.Enter);

            var kycChoice = cmd == '1'
                ? KycChoices.Both
                : cmd == '2'
                    ? KycChoices.NonKyc
                    : KycChoices.Kyc;

            var refPairsForKycChoice = Config.GetPairs(kycChoice);
            var selectedPairs = SetFixing._selectedPairs.Intersect(refPairsForKycChoice);

            foreach (var pair in selectedPairs)
            {
                if (kycChoice == KycChoices.Both) Tempos.AddPairToTempos(pair, Config.GetFixingIntervalsForPair(pair).Merge());
                else if (kycChoice == KycChoices.NonKyc) Tempos.AddPairToTempos(pair, Config.GetFixingIntervalsForPair(pair, KycStatus.NonKYC));
                else Tempos.AddPairToTempos(pair, Config.GetFixingIntervalsForPair(pair, KycStatus.KYC));
            }
        }
    }
}

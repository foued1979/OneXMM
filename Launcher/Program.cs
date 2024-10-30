using Launcher;
using Launcher.MenuFunctions;
using OneXMM;
using OneXMM.Factories;
using OneXMM.Referential;

ExchangeServices.Init();

while (true)
{
    Console.Clear();
    Menu.WriteMainMenu();
    ConsoleKeyInfo key;
    var cmd = "";

    do
    {
        key = Console.ReadKey(true);
        if ((int)key.KeyChar >= 48 && (int)key.KeyChar <= 59)
        {
            Console.Write(key.KeyChar);
            cmd += key.KeyChar;
        }
    } while (key.Key != ConsoleKey.Enter);

    if (!string.IsNullOrEmpty(cmd))
    {
        // Go to Pair selection process
        if (cmd.Equals("1")) SetFixing.LaunchFixingMenu();
    }
}

//var surface = Factory.GetVolatilitySurface(new DateTime(2024, 7, 19, 18, 0, 0, DateTimeKind.Local));

var pair = Config.GetPair("BTC", "USD", Blockchains.Ethereum);
//var pricingTime = new DateTime(2024, 7, 20, 16, 0, 0, DateTimeKind.Local);

//var res = FixingFinder.FindFixing(pricingTime, deribit, surface, PoolMaturities.TwoWeeks);
//Console.WriteLine(res);

//var btcVolSurfaceBuilder = Factory.CreateVolSurfaceBuilder(DateTime.Now.ToUniversalTime(), pair, deribit, futures, options);
//var volSurface = btcVolSurfaceBuilder.Calibrate(FixingIntervals.ThirtyMinutes);

//var fwd = volSurface.GetForward(spot, volSurface.SurfaceDate, volSurface.Smiles.First().Key);

//var vol = volSurface.GetVol(fwd, 67000, volSurface.Smiles.First().Key);
//var selectedOption = options.First(o => o.Expiry.Day == 23 && o.Strike == 67000);
//var optionPrice = selectedOption.Price(DateTime.Now, spot, volSurface);
//Console.WriteLine(vol);

//var surface = btcVolSurfaceBuilder as VolatilitySurface;
//surface.Serialize();
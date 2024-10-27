using OneXMM;
using OneXMM.Factories;
using OneXMM.Pricing;


var contracts = ContractFactory.GetEthereumLikeContracts(OneXMM.Blockchains.HardHat, "0x9fE46736679d2D9a65F0992F2272dE9f3c7fa6e0", true);

//var surface = Factory.GetVolatilitySurface(new DateTime(2024, 7, 19, 18, 0, 0, DateTimeKind.Local));

var pair = Config.GetPair("BTC", "USD", Blockchains.Ethereum);
var deribit = Factory.CreateExchange(Exchanges.Deribit, false);
//var pricingTime = new DateTime(2024, 7, 20, 16, 0, 0, DateTimeKind.Local);

//var res = FixingFinder.FindFixing(pricingTime, deribit, surface, PoolMaturities.TwoWeeks);
//Console.WriteLine(res);

var options = deribit.LoadOptions(pair);
var futures = deribit.LoadFutures(pair);

var btcVolSurfaceBuilder = Factory.CreateVolSurfaceBuilder(DateTime.Now.ToUniversalTime(), pair, deribit, futures, options);
var volSurface = btcVolSurfaceBuilder.Calibrate(VolUpdateTempos.ThirtyMinutes);

var spot = deribit.GetSpot(pair);
var fwd = volSurface.GetForward(spot, volSurface.SurfaceDate, volSurface.Smiles.First().Key);

var vol = volSurface.GetVol(fwd, 67000, volSurface.Smiles.First().Key);
var selectedOption = options.First(o => o.Expiry.Day == 23 && o.Strike == 67000);
var optionPrice = selectedOption.Price(DateTime.Now, spot, volSurface);
Console.WriteLine(vol);

//var surface = btcVolSurfaceBuilder as VolatilitySurface;
//surface.Serialize();

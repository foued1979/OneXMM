using OneXMM;
using OneXMM.Pricing;


var surface = Factory.GetVolatilitySurface(new DateTime(2024, 7, 19, 18, 0, 0, DateTimeKind.Local));

var pair = Config.GetPair("BTC", "USD", Blockchains.Ethereum);
var deribit = Factory.CreateExchange(Exchanges.Deribit, false);
//var pricingTime = new DateTime(2024, 7, 20, 16, 0, 0, DateTimeKind.Local);

//var res = FixingFinder.FindFixing(pricingTime, deribit, surface, PoolMaturities.TwoWeeks);
//Console.WriteLine(res);

var options = deribit.LoadOptions(pair);
var futures = deribit.LoadFutures(pair);

var btcVolSurfaceBuilder = Factory.CreateVolSurfaceBuilder(DateTime.Now, pair, deribit, futures, options);
btcVolSurfaceBuilder.Calibrate(VolUpdateTempos.TwoHours);

//var surface = btcVolSurfaceBuilder as VolatilitySurface;
//surface.Serialize();

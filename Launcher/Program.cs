using OneXMM;
using OneXMM.Factories;
using OneXMM.Pricing;
using OneXMM.Referential;
using OneXMM.Referential.Exchanges;
using System.Diagnostics;

var pair = Config.RefData.GetPair("BTC", "USD");
var deribit = Factory.CreateExchange(Exchanges.Deribit, false);
var options = deribit.LoadOptions("BTC");
var futures = deribit.LoadFutures("BTC");

//var d = DateTime.Now;
//var exp = d.AddDays(2);
//var sabr = new SABR(d, exp);
//sabr.WithParameters(0.6, 0.4, 0.8, 0.1);

//var forward = 103;
//var strike = 105;
//var option = new Option(0, deribit as Exchange, "test", pair, OptionTypes.Call, strike, exp.ToString("ddMMMyy"));

//var price = option.Price(d, forward, sabr);
//Debug.WriteLine(sabr.GetVol(forward, strike));
//Debug.WriteLine(price);
var btcVolSurface = Factory.CreateVolSurface(DateTime.Now, pair, deribit, futures, options);
var sabrs = btcVolSurface.Calibrate(VolUpdateTempos.OneHour);

var spot = 0.0;
var price = 0.0;
var vol = 0.0;

foreach (var opt in options)
    if (Math.Abs(opt.Strike - 65000) < 1000)
    {
        spot = deribit.GetSpot(pair);
        price = opt.Price(DateTime.Now, spot, btcVolSurface);
        break;
    }

Console.WriteLine($"Spot: {spot} -> {price}");

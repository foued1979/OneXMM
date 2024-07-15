using OneXMM;
using OneXMM.Factories;

//var contracts = ContractFactory.GetEthereumLikeContracts(Blockchains.HardHat, true);
//var requestManager = ContractFactory.CreateRequestManager<XMMFunctions>(contracts[0], true);

//var result = requestManager.ExecuteTask(XMMFunctions.add, new object[2] { 5, 6 });
var pair = Config.RefData.GetPair("BTC", "USD");
var deribit = Factory.CreateExchange(Exchanges.Deribit, false);
var options = deribit.LoadOptions("BTC");
var futures = deribit.LoadFutures("BTC");

var btcVolSurface = Factory.CreateVolSurface(DateTime.Now, pair, deribit, futures, options);
btcVolSurface.Calibrate(VolUpdateTempos.OneHour);

Console.WriteLine(options.Length);

using OneXMM;
using OneXMM.Factories;

//var contracts = ContractFactory.GetEthereumLikeContracts(Blockchains.HardHat, true);
//var requestManager = ContractFactory.CreateRequestManager<XMMFunctions>(contracts[0], true);

//var result = requestManager.ExecuteTask(XMMFunctions.add, new object[2] { 5, 6 });
var pair = Config.RefData.GetPair("BTC", "USD");
var deribit = Factory.CreateExchange(Exchanges.Deribit, true);
var options = deribit.LoadOptions("BTC");
var futures = deribit.LoadFutures("BTC");

var btcVolSurface = Factory.CreateVolSurface(DateTime.Now, pair, futures, options);
var spot = deribit.GetSpot(pair);

Console.WriteLine(options.Length);

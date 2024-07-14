using OneXMM;
using OneXMM.Factories;

//var contracts = ContractFactory.GetEthereumLikeContracts(Blockchains.HardHat, true);
//var requestManager = ContractFactory.CreateRequestManager<XMMFunctions>(contracts[0], true);

//var result = requestManager.ExecuteTask(XMMFunctions.add, new object[2] { 5, 6 });
var deribit = Factory.CreateExchange(Exchanges.Deribit, true);
var options = deribit.LoadOptions("BTC");

Console.WriteLine(options.Length);

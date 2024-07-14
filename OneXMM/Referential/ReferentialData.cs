using System;

namespace OneXMM.Referential
{
    using OneXMM.Referential.Assets;

    public class ReferentialData
    {
        public SortedDictionary<string, Assets.Coin> Coins = new SortedDictionary<string, Assets.Coin>();

        public SortedDictionary<string, Assets.Pair> Pairs = new SortedDictionary<string, Assets.Pair>();

        private string[] _pairList = [];
        public string[] ListOfPairs
        {
            get => _pairList;
            set { _pairList = value; foreach (var pair in value) AddPair(pair); }
        }

        public Pair? GetPair(string coinSymbol, string baseSymbol)
        {
            Pairs.TryGetValue($"{coinSymbol}/{baseSymbol}", out var p);
            return p;
        }

        private void AddPair(string pair)
        {
            var assets = pair.Split('/');
            if (assets.Length != 2) throw new ArgumentException($"Bad pair definition: {pair}.");

            var coin = new Coin(assets[0]);
            var counter = new Coin(assets[1]);

            lock (Coins)
            { 
                if (!Coins.ContainsKey(coin.Symbol)) Coins.Add(coin.Symbol, coin);
                if (!Coins.ContainsKey(counter.Symbol)) Coins.Add(counter.Symbol, counter);
            }

            var p = new Pair(coin, counter);

            lock (Pairs)
            {
                if (!Pairs.ContainsKey(p.Symbol)) Pairs.Add(p.Symbol, p);
            }
        }
    }
}

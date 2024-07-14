using System;

namespace OneXMM.Referential
{
    using OneXMM.Configuration;
    using OneXMM.Referential.Assets;

    public class ReferentialData
    {
        public SortedDictionary<string, Assets.Coin> Coins = new SortedDictionary<string, Assets.Coin>();

        public SortedDictionary<string, Assets.Pair> Pairs = new SortedDictionary<string, Assets.Pair>();

        private PairDef[] _pairDefs;
        public PairDef[] PairDefs
        {
            get { return _pairDefs; }
            set { _pairDefs = value; foreach (var pairDef in value) AddPair(pairDef); }
        }

        public Pair? GetPair(string coinSymbol, string baseSymbol)
        {
            Pairs.TryGetValue($"{coinSymbol}/{baseSymbol}", out var p);
            return p;
        }

        private void AddPair(PairDef pairDef)
        {
            if (!Coins.TryGetValue(pairDef.Coin, out var coin)) throw new Exception($"Unknown coin {pairDef.Coin}.");
            if (!Coins.TryGetValue(pairDef.Base, out var counter)) throw new Exception($"Unknown coin {pairDef.Base}.");

            lock (Coins)
            { 
                if (!Coins.ContainsKey(coin.Symbol)) Coins.Add(coin.Symbol, coin);
                if (!Coins.ContainsKey(counter.Symbol)) Coins.Add(counter.Symbol, counter);
            }

            var p = new Pair(coin, counter, pairDef.ExchangeSymbols);

            lock (Pairs)
            {
                if (!Pairs.ContainsKey(p.Symbol)) Pairs.Add(p.Symbol, p);
            }
        }
    }
}

using System;
using Newtonsoft.Json;

namespace OneXMM.Configuration
{
    using OneXMM.Referential;

    internal class StoredFutures
    {
        private readonly static string _filePath = Path.Combine(Environment.CurrentDirectory, "Configuration\\StoredFutures.json");
        public Dictionary<Exchanges, FuturesDetails> AllFutures { get; set; } = new Dictionary<Exchanges, FuturesDetails>();

        public void UpdateFutures(Exchanges exchange, TimeSpan cutoff, Futures[] futures)
        {
            if (!AllFutures.TryGetValue(exchange, out var futuresDetails))
            {
                lock (AllFutures)
                    AllFutures.TryAdd(exchange, new FuturesDetails() { Asof = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).Add(cutoff) });
            }
            else
            {
                futuresDetails.Asof = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).Add(cutoff);
                futuresDetails.Futures = futures;
            }

            var serializer = new JsonSerializer();

            if (File.Exists(_filePath)) File.Delete(_filePath);

            using (var outputFile = new FileStream(_filePath, FileMode.CreateNew))
            using (var writer = new StreamWriter(outputFile))
            {
                serializer.Serialize(writer, this);
            }
        }
    }

    internal class FuturesDetails
    {
        public DateTime Asof { get; set; }
        public Futures[] Futures { get; set; }
        public bool IsUpToDate => Asof.AddDays(1).CompareTo(DateTime.Now.ToUniversalTime()) > 0;

    }
}

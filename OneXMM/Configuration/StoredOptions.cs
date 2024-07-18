using System;
using Newtonsoft.Json;

namespace OneXMM.Configuration
{
    using OneXMM.Referential;

    internal class StoredOptions
    {
        private readonly static string _filePath = Path.Combine(Environment.CurrentDirectory, "Configuration\\StoredOptions.json");
        public Dictionary<Exchanges, OptionDetails> AllOptions { get; set; } = new Dictionary<Exchanges, OptionDetails>();

        public void UpdateOptions(Exchanges exchange, TimeSpan cutoff, Option[] options)
        {
            if (!AllOptions.TryGetValue(exchange, out var optionDetails))
            {
                lock (AllOptions)
                    AllOptions.TryAdd(exchange, new OptionDetails() { Asof = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).Add(cutoff) });
            }
            else
            {
                optionDetails.Asof = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).Add(cutoff);
                optionDetails.Options = options;
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

    internal class OptionDetails
{
        public DateTime Asof { get; set; }
        public Option[] Options { get; set; }
        public bool IsUpToDate => Asof.AddDays(1).CompareTo(DateTime.Now.ToUniversalTime()) > 0;

    }
}

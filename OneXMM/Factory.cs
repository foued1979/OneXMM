using System;

namespace OneXMM
{
    using OneXMM.Communication;
    using OneXMM.Interfaces;
    using OneXMM.Pricing.VolatilitySurfaces;
    using OneXMM.Referential.Assets;

    public static class Factory
    {
        public static IVolatilitySurface? GetVolatilitySurface(DateTime asof)
        {
            var surfaceDate = new DateTime(asof.Year, asof.Month, asof.Day, asof.ToUniversalTime().Hour, 0, 0, DateTimeKind.Utc);
            var path = Path.Combine(Environment.CurrentDirectory, $"Configuration\\VolSurface_{surfaceDate.ToString("dd-MM-yyyyTHH_mm_ss")}.json");

            if (File.Exists(path))
            {
                return Utils.Utils.ConvertFileToObject<VolatilitySurface>(path, "dd-MM-yyyyTHH:mm:ss");
            }

            return null;
        }

        public static double ComputeCorrelatedVol(Pair pair, FixingIntervals updateTempo)
        {
            throw new NotImplementedException();
        }

        public static IDBConnector CreateMySQLConnector()
        {
            var appPath = Path.Combine(AppContext.BaseDirectory, "Configuration\\MySQLSettings.txt");
            var settings = new Dictionary<string, string>();

            using (var fileReader = new StreamReader(appPath))
            {
                var line = "";

                while (!string.IsNullOrEmpty(line = fileReader.ReadLine()))
                {
                    var cmp = line.Split('=');
                    settings.Add(cmp[0], cmp[1]);
                }
            }

            return new MySQLConnector(settings["SERVER"], settings["USER"], settings["PASSWORD"], settings["MARKETDATA_DB"], Convert.ToInt32(settings["PORT"]));
        }
    }
}

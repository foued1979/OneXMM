using System;

namespace OneXMM.Communication.MySQLResults
{
    using OneXMM.Interfaces;
    using OneXMM.Pricing;

    public struct VolSurfaceResult
    {
        public DateTime Timestamp { get; set; }
        public string Symbol { get; set; }
        public string Json { get; set; }

        public IVolatilitySurface ToVolSurface()
        {
            return Utils.Utils.ConvertJsonToObject<VolatilitySurface>(Json) ?? throw new Exception($"VolSurfaceResult - Cannot read json string for pair {Symbol}.");
        }
    }
}

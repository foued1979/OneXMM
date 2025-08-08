using System;

namespace OneXMM.Services
{
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;

    public static class VolatilityServices
    {
        private static SortedDictionary<Pair, IVolatilitySurface?> _volSurfaces = new SortedDictionary<Pair, IVolatilitySurface?>();

        public static void AddPair(Pair pair)
        {
            lock (_volSurfaces)
                if (!_volSurfaces.ContainsKey(pair)) _volSurfaces.Add(pair, null);
        }

        public static void UpdateVolatilitySurface(IVolatilitySurface surface)
        {
            lock (_volSurfaces)
                if (_volSurfaces.ContainsKey(surface.Pair)) _volSurfaces[surface.Pair] = surface;
                else _volSurfaces.Add(surface.Pair, surface);
        }

        public static void GetVolatilityAndForward(Pair pair, double strike, double spot, DateTime expiry, out double fwd, out double vol)
        {
            if (_volSurfaces.TryGetValue(pair, out var volSurface) && volSurface is not null)
            {
                fwd = volSurface.GetForward(spot, expiry);
                vol = volSurface.GetVol(fwd, strike, expiry);

                return;
            }

            throw new Exception($"VolatilityServices - Volurface is missing for pair {pair.Symbol}.");
        }

        public static double GetVolatility(Pair pair, double strike, double spot, DateTime expiry)
        {
            if (_volSurfaces.TryGetValue(pair, out var volSurface) && volSurface is not null)
            {
                var fwd = volSurface.GetForward(spot, expiry);
                return volSurface.GetVol(fwd, strike, expiry);
            }

            throw new Exception($"VolatilityServices - Volurface is missing for pair {pair.Symbol}.");
        }
    }
}

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace OneXMM.Referential.Exchanges.Requests
{
    internal static class RequestFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string CreateDeribitAuthenticationRequest(int id, string clientId, string clientSecret)
            => $"{{\"method\":\"public/auth\",\"params\":{{\"grant_type\":\"client_credentials\",\"client_id\":\"{clientId}\",\"client_secret\":\"{clientSecret}\"}},\"jsonrpc\":\"2.0\",\"id\":{id}}}";

        public static ArraySegment<byte> ToArraySegment(this string s) => new ArraySegment<byte>(Encoding.UTF8.GetBytes(s));
    }
}

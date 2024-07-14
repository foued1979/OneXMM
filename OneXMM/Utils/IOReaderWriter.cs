using System;

namespace OneXMM.Utils
{
    internal static partial class Utils
    {
        public static T? ConvertJsonToObject<T>(string json) where T : class
        {
            using (var stringReader = new StringReader(json))
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(stringReader))
            {
                var serializer = new Newtonsoft.Json.JsonSerializer();

                try
                {
                    return serializer.Deserialize<T>(jsonReader);
                }
                catch
                {
                    return default(T);
                }
            }
        }
    }
}

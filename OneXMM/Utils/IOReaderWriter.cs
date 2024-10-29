using System;

namespace OneXMM.Utils
{
    public static partial class Utils
    {
        public static void Serialize<T>(T objectToSerialize, string filePath, string dateTimeFormat = "") where T : class
        {
            using (var file = File.OpenWrite(filePath))
            using (var streamWriter = new StreamWriter(file))
            using (var jsonWriter = new Newtonsoft.Json.JsonTextWriter(streamWriter))
            {
                var serializer = new Newtonsoft.Json.JsonSerializer();
                if (!string.IsNullOrEmpty(dateTimeFormat)) serializer.DateFormatString = dateTimeFormat;

                try
                {
                    serializer.Serialize(jsonWriter, objectToSerialize);
                }
                catch
                {
                    throw new Exception($"Cannot serialize object type {typeof(T).Name}.");
                }
            }
        }

        public static T? ConvertFileToObject<T>(string filePath, string dateTimeFormat = "") where T : class
        {
            using (var file = File.OpenRead(filePath))
            {
                return ConvertJsonToObject<T>(file, dateTimeFormat);
            }
        }

        public static T? ConvertJsonToObject<T>(Stream jsonStream, string dateTimeFormat = "") where T : class
        {
            using (var stringReader = new StreamReader(jsonStream))
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(stringReader))
            {
                var serializer = new Newtonsoft.Json.JsonSerializer();
                if (!string.IsNullOrEmpty(dateTimeFormat)) serializer.DateFormatString = dateTimeFormat;

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

using Nethereum.Contracts.QueryHandlers.MultiCall;
using System;
using System.Numerics;
using System.Web;

namespace XMMUI
{
    internal static class Tools
    {
        public static object? ToBlockchainObject(this DataGridViewCell cell, string type)
        {
            var cellValue = cell.Value.ToString();
            if (cell.Value is null || string.IsNullOrEmpty(cellValue)) return null;

            if (type.Equals("address")) return cellValue;
            if (type.Contains("[]"))
            {
                var separator = cellValue.Contains(',') ? ',' : ';';
                cellValue = cellValue.TrimEnd(separator);

                var values = cellValue.Split(separator);
                var res = new object[values.Length];

                for (int i = 0; i < values.Length; ++i)
                {
                    var value = values[i];
                    if (type.Contains("uint")) res[i] = ConvertToUInt(type, cellValue);
                    if (type.Contains("int")) res[i] = ConvertToInt(type, cellValue);
                }

                return res;
            }
            if (type.Contains("uint")) return ConvertToUInt(type, cellValue);
            if (type.Contains("int")) return ConvertToInt(type, cellValue);

            throw new NotImplementedException();
        }

        private static object ConvertToUInt(string type, string value)
        {
            if (type.Length > 4)
            {
                var size = Convert.ToUInt16(type.Substring(4));

                if (size <= 8) return Convert.ToByte(value);
                if (size <= 16) return Convert.ToUInt16(value);
                if (size <= 32) return Convert.ToUInt32(value);
                if (size <= 64) return Convert.ToUInt64(value);
            }

            return BigInteger.Parse(value);
        }

        private static object ConvertToInt(string type, string value)
        {
            if (type.Length > 4)
            {
                var size = Convert.ToUInt16(type.Substring(4));

                if (size <= 16) return Convert.ToInt16(value);
                if (size <= 32) return Convert.ToInt32(value);
                if (size <= 64) return Convert.ToInt64(value);
            }

            return BigInteger.Parse(value);
        }
    }
}

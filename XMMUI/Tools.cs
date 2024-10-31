using Nethereum.Contracts.QueryHandlers.MultiCall;
using System;
using System.Numerics;

namespace XMMUI
{
    using XMMUI.Content;

    internal static class Tools
    {
        public static object? ToBlockchainObject(this DataGridViewCell cell, ParameterDescription paramDescr, bool auto_manage_decimals)
        {
            var cellValue = cell.Value.ToString();
            if (cell.Value is null || string.IsNullOrEmpty(cellValue)) return null;

            var type = paramDescr.ParameterType;

            if (type.Equals("address")) return cellValue;
            if (type.Equals("address[]"))
            {
                var separator = cellValue.Contains(',') ? ',' : ';';
                cellValue = cellValue.TrimEnd(separator);

                return cellValue.Split(separator);
            }
            if (type.Contains("[]"))
            {
                var separator = cellValue.Contains(',') ? ',' : ';';
                cellValue = cellValue.TrimEnd(separator);

                var values = cellValue.Split(separator);
                var res = new object[values.Length];

                for (int i = 0; i < values.Length; ++i)
                {
                    var value = values[i];
                    if (type.Contains("uint")) res[i] = ConvertToUInt(type, cellValue, auto_manage_decimals);
                    else if (type.Contains("int")) res[i] = ConvertToInt(type, cellValue, auto_manage_decimals);
                    else if (type.Equals("double")) res[i] = Convert.ToUInt64(Convert.ToDouble(cellValue) * paramDescr.Precision); 
                }

                return res;
            }
            if (type.Contains("uint")) return ConvertToUInt(type, cellValue, auto_manage_decimals);
            if (type.Contains("int")) return ConvertToInt(type, cellValue, auto_manage_decimals);
            if (type.Equals("double")) return Convert.ToUInt64(Convert.ToDouble(cellValue) * paramDescr.Precision);

            throw new NotImplementedException();
        }

        private static object ConvertToUInt(string type, string value, bool auto_manage_decimals)
        {
            if (type.Length > 4)
            {
                var size = Convert.ToUInt16(type.Substring(4));

                if (size <= 8) return Convert.ToByte(value);
                if (size <= 16) return Convert.ToUInt16(value);
                if (size <= 32) return Convert.ToUInt32(value);
                if (size <= 64) return Convert.ToUInt64(value);
            }

            if (auto_manage_decimals)
            {
                return new BigInteger(Convert.ToDecimal(value) * 10000) * new BigInteger(1E14m);
            }
            
            return BigInteger.Parse(value);
        }

        private static object ConvertToInt(string type, string value, bool auto_manage_decimals)
        {
            if (type.Length > 4)
            {
                var size = Convert.ToUInt16(type.Substring(4));

                if (size <= 16) return Convert.ToInt16(value);
                if (size <= 32) return Convert.ToInt32(value);
                if (size <= 64) return Convert.ToInt64(value);
            }

            if (auto_manage_decimals)
            {
                return new BigInteger(Convert.ToDecimal(value) * 10000) * new BigInteger(1E14m);
            }

            return BigInteger.Parse(value);
        }
    }
}

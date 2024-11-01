using System;
using System.Text;

namespace OneXMM.Communication
{
    internal static class TableDef
    {
        public static readonly TableField[] HistoricalPrices = [
            new TableField("symbol", "varchar(30) not null"),
            new TableField("date", "date datetime not null"),
            new TableField("price", "double not null")
            ];

        public static readonly TableField[] FuturesRef = [
            new TableField("id", "int not null auto_increment"),
            new TableField("symbol", "varchar(40) not null"),
            new TableField("undl", "varchar(20) not null"),
            new TableField("expiry", "datetime not null"),
            new TableField("exchange", "varchar(20) not null"),
            new TableField("inputdate", "datetime not null")
            ];

        public static readonly TableField[] OptionsRef = [
            new TableField("id", "int not null auto_increment"),
            new TableField("symbol", "varchar(40) not null"),
            new TableField("optiontype", "varchar(4) not null"),
            new TableField("strike", "double not null"),
            new TableField("undl", "varchar(20) not null"),
            new TableField("expiry", "datetime not null"),
            new TableField("exchange", "varchar(20) not null"),
            new TableField("inputdate", "datetime not null")
            ];

        public static readonly TableField[] Transactions = [
            new TableField("timestamp", "datetime not null"),
            new TableField("transactionHash", "varchar(80) not null"),
            new TableField("transactionIndex", "varchar(34) not null"),
            new TableField("blockHash", "varchar(100) not null"),
            new TableField("blockNumber", "varchar(34) not null"),
            new TableField("_from", "varchar(44)"),
            new TableField("_to", "varchar(44)"),
            new TableField("cumulativeGasUsed", "varchar(34) not null"),
            new TableField("gasUsed", "varchar(34) not null"),
            new TableField("effectiveGasPrice", "varchar(34) not null"),
            new TableField("contractAddress", "varchar(44) not null"),
            ];

        public static string GetFieldNamesWithoutId(string tableName)
        {
            TableField[]? fields = null;

            if (tableName.Equals(MySQLTables.futures_ref.ToString())) fields = FuturesRef;
            else if (tableName.Equals(MySQLTables.options_ref.ToString())) fields = OptionsRef;

            if (fields is null) return string.Empty;

            var sb = new StringBuilder();

            foreach (var field in fields)
            {
                if (field.ColumnName.ToLower().Equals("id")) continue;
                sb.Append($"{field.ColumnName},");
            }

            return sb.ToString().TrimEnd(',');
        }
    }

    internal struct TableField
    {
        public string ColumnName { get; }
        public string Definition { get; }

        public TableField(string columnName,string definition) { ColumnName = columnName; Definition = definition; }
    }
}

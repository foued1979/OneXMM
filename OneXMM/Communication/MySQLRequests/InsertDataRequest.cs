using System;
using System.Text;

namespace OneXMM.Communication.MySQLRequests
{
    internal sealed class InsertDataRequest
    {
        public string TableName { get; }
        public string[][] DataToInsert { get; }

        public InsertDataRequest(string tableName, string[][] dataToInsert)
        {
            TableName = tableName;
            DataToInsert = dataToInsert;
        }

        public string[] ToCmd()
        {
            var cmds = new string[DataToInsert.GetLength(0)];

            for (int i = 0; i < DataToInsert.GetLength(0); i++)
            {
                var sb = new StringBuilder();
                sb.Append($"INSERT INTO {TableName} VALUES(");

                for (int j = 0; j < DataToInsert[i].Length - 1; j++) sb.Append($"'{DataToInsert[i][j]}', ");
                sb.Append($"'{DataToInsert[i].Last()}');");

                cmds[i] = sb.ToString();
            }

            return cmds;
        }
    }
}

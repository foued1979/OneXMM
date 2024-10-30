using System;
using System.Text;

namespace OneXMM.Communication.MySQLRequests
{
    internal sealed class InsertDataRequest
    {
        public string TableName { get; }
        public string[][] DataToInsert { get; }
        public MySQLInsertTypes InsertType { get; }

        public InsertDataRequest(string tableName, string[][] dataToInsert, MySQLInsertTypes insertType)
        {
            TableName = tableName;
            DataToInsert = dataToInsert;
            InsertType = insertType;
        }

        public string[] ToCmd()
        {
            var cmds = new string[DataToInsert.GetLength(0)];

            for (int i = 0; i < DataToInsert.GetLength(0); i++)
            {
                var sb = new StringBuilder();

                switch (InsertType)
                {
                    case MySQLInsertTypes.None:
                    case MySQLInsertTypes.IdInDataToInsert:
                        sb.Append($"INSERT INTO {TableName} VALUES(");
                        break;
                    case MySQLInsertTypes.IdNotInDataToInsert:
                        sb.Append($"INSERT INTO {TableName} ({TableDef.GetFieldNamesWithoutId(TableName)}) VALUES(");
                        break;
                }

                for (int j = 0; j < DataToInsert[i].Length - 1; j++) sb.Append($"'{DataToInsert[i][j]}', ");
                sb.Append($"'{DataToInsert[i].Last()}');");

                cmds[i] = sb.ToString();

            }

            return cmds;
        }
    }
}

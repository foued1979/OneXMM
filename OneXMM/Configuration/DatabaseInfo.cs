using System;

namespace OneXMM.Configuration
{
    public sealed class DatabaseInfo
    {
        public Dictionary<MySQLTables, string> TableMapping { get; set; } = new Dictionary<MySQLTables, string>();
    }
}

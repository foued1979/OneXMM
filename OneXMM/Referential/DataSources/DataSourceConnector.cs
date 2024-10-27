using System;

namespace OneXMM.Referential.DataSources
{
    internal abstract class DataSourceConnector
    {
        protected HttpClient _client;

        protected DataSourceConnector()
        {

        }
    }
}

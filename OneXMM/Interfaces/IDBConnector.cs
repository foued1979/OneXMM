using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Communication.MySQLRequests;

    public interface IDBConnector
    {
        IEnumerable<TResult> Get<TResult>(MySQLRequest<TResult> request) where TResult : struct;

        void InsertData(InsertDataRequest insertRequest);
    }
}

using System;

namespace OneXMM.Communication
{
    using OneXMM.Interfaces;

    internal abstract partial class RequestManager<FType> : IRequestManager<FType>
        where FType : Enum
    {
        protected readonly SortedDictionary<FType, bool> _queueMapping = new SortedDictionary<FType, bool>();
        protected readonly SortedDictionary<FType, dynamic?> _lastResult = new SortedDictionary<FType, dynamic?>();

        protected RequestManager()
        {
            foreach (FType functionType in Enum.GetValues(typeof(FType)))
            {
                _queueMapping.Add(functionType, false);
                _lastResult.Add(functionType, null);
            }
        }

        public abstract dynamic ExecuteTask(FType functionType, params object[] parameters);
    }
}

using System;

namespace OneXMM.Interfaces
{
    public interface IRequestManager<FType>
        where FType : Enum
    {
        public dynamic ExecuteTask(FType functionType, params object[] parameters);
    }
}

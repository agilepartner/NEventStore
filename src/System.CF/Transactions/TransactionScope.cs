using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace System.Transactions
{
    public class TransactionScope : IDisposable
    {
        // TODO : Implement decent TransactionScope

        public TransactionScope(TransactionScopeOption option)
        {
            
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Complete()
        {
            throw new NotImplementedException();
        }
    }
}

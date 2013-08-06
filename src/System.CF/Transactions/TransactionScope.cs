using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace System.Transactions
{
    public class TransactionScope : IDisposable
    {

        public TransactionScope(TransactionScopeOption option)
        {
            // TODO : Implement decent TransactionScope
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

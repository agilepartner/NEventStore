using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace NEventStore.Example.CF
{
    public class PipelineHook : IPipelineHook
    {
        public Commit Select(Commit committed)
        {
            Console.WriteLine("Selecting committed {0}", committed.CommitId);
            return committed;
        }

        public bool PreCommit(Commit attempt)
        {
            Console.WriteLine("Before committing {0}", attempt.CommitId);
            return true;
        }

        public void PostCommit(Commit committed)
        {
            Console.WriteLine("Committed {0}", committed.CommitId);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            // Dispose stuff there
        }
    }
}

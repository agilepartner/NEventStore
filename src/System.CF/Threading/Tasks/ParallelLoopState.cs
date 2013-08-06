using System;

namespace System.Threading.Tasks
{
    public class ParallelLoopState
    {
        internal ParallelLoopState()
        {

        }

        public long? LowestBreakIteration
        {
            get;
            private set;
        }

        public void Break()
        {

        }

        public void Stop()
        {

        }
    }
}

using System;

namespace System.Threading.Tasks
{
    public struct ParallelLoopResult
    {
        public bool IsCompleted
        {
            get;
            internal set;
        }

        public long? LowestBreakIteration
        {
            get;
            internal set;
        }
    }
}

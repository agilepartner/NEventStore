using System;

namespace System.Threading.Tasks
{
    public static class Parallel
    {
        public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            return ForWorker<object>(fromInclusive, toExclusive, body, null);
        }

        public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int, ParallelLoopState> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            return ForWorker<object>(fromInclusive, toExclusive, null, body);
        }

        private static ParallelLoopResult ForWorker<T>(int fromInclusive, int toExclusive, Action<int> body, Action<int, ParallelLoopState> bodyWithState)
        {
            var result = new ParallelLoopResult();
            result.IsCompleted = true;

            var tasks = new Task[toExclusive - fromInclusive];

            try
            {
                if (body != null)
                {
                    for (int i = fromInclusive; i < toExclusive; i++)
                    {
                        var index = i;
                        tasks[i] = Task.Factory.StartNew(() => body(index));
                    }
                }

                else
                {
                    var loopState = new ParallelLoopState();

                    for (int i = fromInclusive; i < toExclusive; i++)
                    {
                        var index = i;
                        tasks[i] = Task.Factory.StartNew(() => bodyWithState(index, loopState));
                    }
                }

                Task.WaitAll(tasks);
            }

            catch(AggregateException)
            {
                result.IsCompleted = false;
            }

            return result;
        }
    }
}

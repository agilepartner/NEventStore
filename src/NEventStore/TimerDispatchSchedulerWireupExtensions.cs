namespace NEventStore
{
    using System;
    using NEventStore.Dispatcher;

    public static class TimerDispatchSchedulerWireupExtensions
    {
        public static TimerDispatchSchedulerWireup UsingTimerDispatchScheduler(this Wireup wireup, TimeSpan delayBeforeStart, TimeSpan interval)
        {
            return wireup.UsingTimerDispatchScheduler(null, delayBeforeStart, interval);
        }

        public static TimerDispatchSchedulerWireup UsingTimerDispatchScheduler(
            this Wireup wireup, IDispatchCommits dispatcher, TimeSpan delayBeforeStart, TimeSpan interval)
        {
            return new TimerDispatchSchedulerWireup(wireup, dispatcher, delayBeforeStart, interval);
        }
    }
}

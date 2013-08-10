namespace NEventStore
{
    using System;
    using System.Transactions;
    using NEventStore.Dispatcher;
    using NEventStore.Logging;
    using NEventStore.Persistence;

    public class TimerDispatchSchedulerWireup : Wireup
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(TimerDispatchSchedulerWireup));

        public TimerDispatchSchedulerWireup(Wireup wireup, IDispatchCommits dispatcher, TimeSpan delayBeforeStart, TimeSpan interval)
            : base(wireup)
        {
            var option = Container.Resolve<TransactionScopeOption>();
            if (option != TransactionScopeOption.Suppress)
            {
                Logger.Warn(Messages.SynchronousDispatcherTwoPhaseCommits);
            }

            Logger.Debug(Messages.TimerDispatchSchedulerRegistered);
            DispatchTo(dispatcher ?? new NullDispatcher());
            Container.Register<IScheduleDispatches>(c => new TimerDispatchScheduler(
                c.Resolve<IDispatchCommits>(), c.Resolve<IPersistStreams>(), delayBeforeStart, interval));
        }
        
        public TimerDispatchSchedulerWireup DispatchTo(IDispatchCommits instance)
        {
            Logger.Debug(Messages.DispatcherRegistered, instance.GetType());
            Container.Register(instance);
            return this;
        }
    }
}

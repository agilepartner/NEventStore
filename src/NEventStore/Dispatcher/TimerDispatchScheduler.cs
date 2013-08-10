namespace NEventStore.Dispatcher
{
    using System;
    using System.Threading;
    using NEventStore.Logging;
    using NEventStore.Persistence;
using System.Collections.Generic;

    public class TimerDispatchScheduler : IScheduleDispatches
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(TimerDispatchScheduler));

        private readonly IDispatchCommits _dispatcher;
        private readonly IPersistStreams _persistence;
        private readonly Timer _timer;
		private readonly Queue<Commit> _queue;
        private bool _disposed;

        public TimerDispatchScheduler(IDispatchCommits dispatcher, IPersistStreams persistence, TimeSpan delayBeforeStart, TimeSpan interval)
        {
            _dispatcher = dispatcher;
            _persistence = persistence;

            Logger.Debug(Resources.InitializingPersistence);
            _persistence.Initialize();

			Logger.Debug(Resources.GettingUndispatchedCommits);
			_queue = new Queue<Commit>(_persistence.GetUndispatchedCommits());

            Logger.Info(Resources.StartingDispatchScheduler);
            _timer = new Timer(new TimerCallback(Working), null, delayBeforeStart, interval);
        }

        private void Working(object state)
        {
			lock(_queue) 
			{
				while(_queue.Count > 0) 
				{
					var commit = _queue.Dequeue();
					if (DispatchImmediately(commit))
						MarkAsDispatched(commit);
				}
			}
        }

        public void ScheduleDispatch(Commit commit)
        {
            Logger.Info(Resources.SchedulingDelivery, commit.CommitId);
			lock(_queue)
				_queue.Enqueue(commit);
        }

        private bool DispatchImmediately(Commit commit)
        {
            try
            {
                Logger.Info(Resources.SchedulingDispatch, commit.CommitId);
                return _dispatcher.Dispatch(commit);
            }
            catch
            {
                Logger.Error(Resources.UnableToDispatch, _dispatcher.GetType(), commit.CommitId);
                throw;
            }
        }

        private void MarkAsDispatched(Commit commit)
        {
            try
            {
                Logger.Info(Resources.MarkingCommitAsDispatched, commit.CommitId);
                _persistence.MarkCommitAsDispatched(commit);
            }
            catch (ObjectDisposedException)
            {
                Logger.Warn(Resources.UnableToMarkDispatched, commit.CommitId);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            Logger.Debug(Resources.ShuttingDownDispatchScheduler);
            _dispatcher.Dispose();
            _persistence.Dispose();
            _timer.Dispose();
            _disposed = true;
        }


    }
}

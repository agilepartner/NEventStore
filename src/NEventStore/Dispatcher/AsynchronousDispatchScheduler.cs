namespace NEventStore.Dispatcher
{
#if PocketPC
    using System;
#endif
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using NEventStore.Logging;
    using NEventStore.Persistence;

    public class AsynchronousDispatchScheduler : SynchronousDispatchScheduler
    {
        private const int BoundedCapacity = 1024;
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (AsynchronousDispatchScheduler));
        private BlockingCollection<Commit> _queue;
        private Task _worker;
        private bool _working;

        public AsynchronousDispatchScheduler(IDispatchCommits dispatcher, IPersistStreams persistence)
            : base(dispatcher, persistence)
        {}

        protected override void Start()
        {
#if PocketPC
            _queue = new BlockingCollection<Commit>(new System.Collections.Generic.Queue<Commit>(), BoundedCapacity);
#else
            _queue = new BlockingCollection<Commit>(new ConcurrentQueue<Commit>(), BoundedCapacity); 
#endif
            _worker = new Task(Working);
            _working = true;
            _worker.Start();

            base.Start();
        }

        public override void ScheduleDispatch(Commit commit)
        {
            Logger.Info(Resources.SchedulingDelivery, commit.CommitId);
            _queue.Add(commit);
        }

        private void Working()
        {
            while (_working)
            {
                Commit commit;
#if PocketPC
                if (_queue.TryTake(out commit))
                {
                    base.ScheduleDispatch(commit);
                }
#else
                if (_queue.TryTake(out commit, 100))
                {
                    base.ScheduleDispatch(commit);
                } 
#endif
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _working = false;
        }
    }
}
namespace NEventStore.Diagnostics
{
    using System;
    using System.Collections.Generic;
#if !PocketPC
    using System.Diagnostics; 
#endif
    using NEventStore.Persistence;

    public class PerformanceCounterPersistenceEngine : IPersistStreams
    {
#if !PocketPC
        private readonly PerformanceCounters _counters;
#endif        
        private readonly IPersistStreams _persistence;

        public PerformanceCounterPersistenceEngine(IPersistStreams persistence, string instanceName)
        {
            _persistence = persistence;
#if !PocketPC
            _counters = new PerformanceCounters(instanceName); 
#endif
        }

        public virtual void Initialize()
        {
            _persistence.Initialize();
        }

        public virtual void Commit(Commit attempt)
        {
#if PocketPC
            _persistence.Commit(attempt);
#else
            Stopwatch clock = Stopwatch.StartNew();
            _persistence.Commit(attempt);
            clock.Stop();

            _counters.CountCommit(attempt.Events.Count, clock.ElapsedMilliseconds); 
#endif
        }

        public virtual void MarkCommitAsDispatched(Commit commit)
        {
            _persistence.MarkCommitAsDispatched(commit);
#if !PocketPC
            _counters.CountCommitDispatched(); 
#endif
        }

        public IEnumerable<Commit> GetFromTo(DateTime start, DateTime end)
        {
            return _persistence.GetFromTo(start, end);
        }

        public virtual IEnumerable<Commit> GetUndispatchedCommits()
        {
            return _persistence.GetUndispatchedCommits();
        }

        public virtual IEnumerable<Commit> GetFrom(Guid streamId, int minRevision, int maxRevision)
        {
            return _persistence.GetFrom(streamId, minRevision, maxRevision);
        }

        public virtual IEnumerable<Commit> GetFrom(DateTime start)
        {
            return _persistence.GetFrom(start);
        }

        public virtual bool AddSnapshot(Snapshot snapshot)
        {
            bool result = _persistence.AddSnapshot(snapshot);
            if (result)
            {
#if !PocketPC
                _counters.CountSnapshot(); 
#endif
            }

            return result;
        }

        public virtual Snapshot GetSnapshot(Guid streamId, int maxRevision)
        {
            return _persistence.GetSnapshot(streamId, maxRevision);
        }

        public virtual IEnumerable<StreamHead> GetStreamsToSnapshot(int maxThreshold)
        {
            return _persistence.GetStreamsToSnapshot(maxThreshold);
        }

        public virtual void Purge()
        {
            _persistence.Purge();
        }

        public bool IsDisposed
        {
            get { return _persistence.IsDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PerformanceCounterPersistenceEngine()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

#if !PocketPC
            _counters.Dispose(); 
#endif
            _persistence.Dispose();
        }
    }
}
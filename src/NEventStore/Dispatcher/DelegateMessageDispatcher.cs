namespace NEventStore.Dispatcher
{
    using System;

    public class DelegateMessageDispatcher : IDispatchCommits
    {
        private readonly Func<Commit, bool> _dispatch;

        public DelegateMessageDispatcher(Func<Commit, bool> dispatch)
        {
            _dispatch = dispatch;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual bool Dispatch(Commit commit)
        {
            return _dispatch(commit);
        }

        protected virtual void Dispose(bool disposing)
        {
            // no op
        }
    }
}
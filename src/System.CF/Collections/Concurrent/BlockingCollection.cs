using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenNETCF.Threading;

namespace System.Collections.Concurrent
{
    public class BlockingCollection<T> : IDisposable
    {
        private readonly Queue<T> _queue;
        private readonly int _maxSize;
        private readonly Monitor2 _monitor;
        private bool _closing;

        public BlockingCollection(Queue<T> innerQueue, int maxSize)
        {
            _queue = innerQueue;
            _maxSize = maxSize;
            _monitor = new Monitor2();
        }

        public void Close()
        {
            lock (_queue)
            {
                _closing = true;
                _monitor.PulseAll();
            }
        }

        public void Add(T item)
        {
            lock (_queue)
            {
                while (_queue.Count >= _maxSize)
                {
                    _monitor.Wait();
                }
                _queue.Enqueue(item);
                if (_queue.Count == 1)
                {
                    // wake up any blocked dequeue
                    _monitor.PulseAll();
                }
            }
        }

        public bool TryTake(out T value)
        {
            lock (_queue)
            {
                while (_queue.Count == 0)
                {
                    if (_closing)
                    {
                        value = default(T);
                        return false;
                    }
                    _monitor.Wait();
                }
                value = _queue.Dequeue();
                if (_queue.Count == _maxSize - 1)
                {
                    // wake up any blocked enqueue
                    _monitor.PulseAll();
                }
                return true;
            }
        }


        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Close();
                }
            }
            _disposed = true;
        }

    }
}

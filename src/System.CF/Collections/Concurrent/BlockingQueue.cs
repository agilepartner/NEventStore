using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OpenNETCF.Threading;

namespace System.Collections.Concurrent
{
    public class BlockingQueue<T>
    {
        private Queue<T> _queue = new Queue<T>();
        private Semaphore _semaphore = new Semaphore(0, int.MaxValue);

        public void Add(T data)
        {
            if (data == null) throw new ArgumentNullException("data");
            lock (_queue) _queue.Enqueue(data);
            _semaphore.Release();
        }

        public bool TryTake(out T data) 
        {
            if (_queue.Count == 0)
            {
                data = default(T);
                return false;
            }
            data = Take();
            return true;
        }

        public T Take()
        {
            _semaphore.WaitOne();
            lock (_queue) return _queue.Dequeue();
        }
    }
}

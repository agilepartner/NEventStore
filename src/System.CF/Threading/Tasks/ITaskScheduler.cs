using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading.Tasks
{
    public interface ITaskScheduler
    {
        void QueueTask(Task task);
    }
}

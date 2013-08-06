using System;

namespace System.Threading.Tasks
{
    public class TaskScheduler : ITaskScheduler
    {
        private static ITaskScheduler _current;
        public static ITaskScheduler Current
        {
            get { return _current ?? (_current = Default); }
            set { _current = value; }
        }

        public static ITaskScheduler Default
        {
            get { return new TaskScheduler(); }
        }

        public void QueueTask(Task task)
        {
            //Use the ThreadPool to queue the work item for the moment
            //I have seen some articles saying that the Xbox 360 Compact Framework ThreadPool doesn't take into account
            //the 4 available hardware threads but the official documentation say the opposite.
            //See http://msdn.microsoft.com/en-us/library/bb203914.aspx
            ThreadPool.QueueUserWorkItem((threadState) => task.InnerInvoke());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an asynchronous operation.
    /// </summary>
    public class Task : IDisposable
    {
        // Private fields
        private object action;
        internal object state;
        private ManualResetEvent completedEvent;
        private Queue<Task> tasksToContinue;
        private Queue<Task> childTasks;

        /// <summary>
        /// Default static constructor.
        /// </summary>
        static Task()
        {
            Factory = new TaskFactory();
        }

        /// <summary>
        /// Initializes a new Task with the specified action.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the Task.</param>
        public Task(Action action)
            : this(action, null, TaskCreationOptions.None)
        {

        }

        public Task(Action action, TaskCreationOptions creationOptions)
            : this(action, null, creationOptions)
        {

        }

        /// <summary>
        /// Initializes a new Task with the specified action and state.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the task.</param>
        /// <param name="state">An object representing data to be used by the action.</param>
        public Task(Action<object> action, object state)
            : this((object)action, null, TaskCreationOptions.None)
        {

        }

        /// <summary>
        /// Internal constructor used by the generic version of the Task object.
        /// </summary>
        /// <param name="action">The delegate that represents the code to execute in the task.</param>
        /// <param name="state">An object representing data to be used by the action.</param>
        internal Task(object action, object state, TaskCreationOptions creationOptions)
        {
            this.action = action;
            this.state = state;
            this.completedEvent = new ManualResetEvent(false);

            if ((creationOptions & TaskCreationOptions.AttachedToParent) != TaskCreationOptions.None && Task.InternalCurrent != null)
            {
                Task.InternalCurrent.AddNewChild(this);
            }
        }

        public AggregateException Exception
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether this Task has completed.
        /// </summary>
        /// <returns>true if the task has completed; otherwise false.</returns>
        public bool IsCompleted
        {
            get
            {
                return this.Status == TaskStatus.RanToCompletion || this.Status == TaskStatus.Faulted || this.Status == TaskStatus.Canceled;
            }
        }

        /// <summary>
        /// Gets whether this Task has faulted.
        /// </summary>
        /// <returns>true if the task has faulted; otherwise false.</returns>
        public bool IsFaulted
        {
            get
            {
                return this.Status == TaskStatus.Faulted;
            }
        }


        public TaskStatus Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Provides access to factory methods for creating Task and Task instances.
        /// </summary>
        /// <returns>The default TaskFactory for the current task.</returns>
        public static TaskFactory Factory
        {
            get;
            private set;
        }

        internal static Task InternalCurrent
        {
            get
            {
                return ThreadLocals.CurrentTask;
            }
        }

        internal void AddNewChild(Task task)
        {
            if (this.childTasks == null)
            {
                this.childTasks = new Queue<Task>();
            }

            this.childTasks.Enqueue(task);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this.IsCompleted)
                {
                    throw new InvalidOperationException("Task is not completed.");
                }

                if (this.completedEvent != null)
                {
                    this.completedEvent = null;
                }
            }

            this.Status = TaskStatus.Canceled;
        }

        public Task ContinueWith(Action<Task> continuationAction)
        {
            if (this.tasksToContinue == null)
            {
                this.tasksToContinue = new Queue<Task>();
            }

            var task = new Task(continuationAction, null, TaskCreationOptions.None);

            this.tasksToContinue.Enqueue(task);

            return task;
        }

        public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction)
        {
            if (this.tasksToContinue == null)
            {
                this.tasksToContinue = new Queue<Task>();
            }

            var task = new Task<TResult>(continuationFunction, this);

            this.tasksToContinue.Enqueue(task);

            return task;
        }

        public void RunSynchronously()
        {
            this.InnerInvoke();
        }

        /// <summary>
        /// Starts the Task, scheduling it for execution to the current TaskScheduler.
        /// </summary>
        public void Start()
        {

        }

        /// <summary>
        /// Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution.
        /// </summary>
        public void Wait()
        {
            this.Wait(-1);
        }

        /// <summary>
        /// Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to 
        /// wait, or a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.
        /// </param>
        /// <returns>
        /// true if the <see cref="T:System.Threading.Tasks.Task" /> completed execution within the allotted 
        /// time; otherwise, false.
        /// </returns>
        public bool Wait(TimeSpan timeout)
        {
            return this.Wait((int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Waits for the <see cref="T:System.Threading.Tasks.Task" /> to complete execution.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> 
        /// (-1) to wait indefinitely.
        /// </param>
        /// <returns>
        /// true if the <see cref="T:System.Threading.Tasks.Task" /> completed execution within 
        /// the allotted time; otherwise, false.
        /// </returns>
        public bool Wait(int millisecondsTimeout)
        {
            var result = true;

            if (!this.IsCompleted && !this.IsFaulted)
            {
                result = this.completedEvent.WaitOne(millisecondsTimeout, false);
            }

            if (this.IsFaulted)
            {
                throw this.Exception;
            }

            return result;
        }

        /// <summary>
        /// Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects 
        /// to complete execution.
        /// </summary>
        /// <param name="tasks">
        /// An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.
        /// </param>
        public static void WaitAll(Task[] tasks)
        {
            Task.WaitAll(tasks, -1);
        }

        /// <summary>
        /// Waits for all of the provided <see cref="T:System.Threading.Tasks.Task" /> objects to complete execution.
        /// </summary>
        /// <param name="tasks">
        /// An array of <see cref="T:System.Threading.Tasks.Task" /> instances on which to wait.
        /// </param>
        /// <param name="timeout">
        /// A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or
        /// a <see cref="T:System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.
        /// </param>
        /// <returns>
        /// true if all of the <see cref="T:System.Threading.Tasks.Task" /> instances completed execution within the 
        /// allotted time; otherwise, false.
        /// </returns>
        public static bool WaitAll(Task[] tasks, TimeSpan timeout)
        {
            return Task.WaitAll(tasks, (int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Waits for all of the provided Task objects to complete execution.
        /// </summary>
        /// <param name="tasks">An array of Task instances on which to wait.</param>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or Timeout.Infinite to wait indefinitely.
        /// </param>
        /// <returns>
        /// true if all of the Task instances completed execution within the allotted time; otherwise, false.
        /// </returns>
        public static bool WaitAll(Task[] tasks, int millisecondsTimeout)
        {
            var result = true;

            for (int i = 0; i < tasks.Length; i++)
            {
                result |= tasks[i].completedEvent.WaitOne(millisecondsTimeout, false);
            }

            var exceptions = new List<Exception>();

            for (int i = 0; i < tasks.Length; i++)
            {
                if (tasks[i].IsFaulted)
                {
                    exceptions.AddRange(tasks[i].Exception.InnerExceptions);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            return result;
        }

        public static int WaitAny(Task[] tasks)
        {
            return WaitAny(tasks, -1);
        }

        public static int WaitAny(Task[] tasks, TimeSpan timeout)
        {
            return Task.WaitAny(tasks, (int)timeout.TotalMilliseconds);
        }

        public static int WaitAny(Task[] tasks, int millisecondsTimeout)
        {
            var waitHandles = new WaitHandle[tasks.Length];

            for (int i = 0; i < tasks.Length; i++)
            {
                waitHandles[i] = tasks[i].completedEvent;
            }

            var result = OpenNETCF.Threading.EventWaitHandle.WaitAny(waitHandles, millisecondsTimeout, false);

            //Catch an exception if one of the tasks in an error state
            var exceptions = new List<Exception>();

            for (int i = 0; i < tasks.Length; i++)
            {
                if (tasks[i].IsFaulted)
                {
                    exceptions.AddRange(tasks[i].Exception.InnerExceptions);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            return result;
        }

        /// <summary>
        /// Do the actual task execution.
        /// </summary>
        internal void InnerInvoke()
        {
            try
            {
                Task.ThreadLocals.CurrentTask = this;

                this.Status = TaskStatus.Running;
                Action action = this.action as Action;

                if (action != null)
                {
                    action();
                }
                else
                {
                    Action<object> action2 = this.action as Action<object>;

                    if (action2 != null)
                    {
                        action2(this.state);
                    }

                    else
                    {
                        var action3 = this.action as Action<Task>;
                        action3(this.state as Task);
                    }
                }

                //Wait for child tasks to complete
                if (this.childTasks != null)
                {
                    while (this.childTasks.Count > 0)
                    {
                        var task = this.childTasks.Dequeue();
                        task.Wait();
                    }
                }

                this.Status = TaskStatus.RanToCompletion;
                this.completedEvent.Set();

                if (this.tasksToContinue != null)
                {
                    while (this.tasksToContinue.Count > 0)
                    {
                        var task = this.tasksToContinue.Dequeue();
                        task.Start();
                    }
                }
            }

            catch (Exception e)
            {
                this.Exception = new AggregateException("The task had an exception.", e);

                this.Status = TaskStatus.Faulted;
                this.completedEvent.Set();
            }
        }

        private static class ThreadLocals
        {
            private static LocalDataStoreSlot _localSlot = Thread.AllocateDataSlot();

            internal static Task CurrentTask
            {
                get { return Thread.GetData(_localSlot) as Task; }
                set { Thread.SetData(_localSlot, value); }
            }
        }
    }
}

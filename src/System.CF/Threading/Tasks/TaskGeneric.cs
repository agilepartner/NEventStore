using System;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an asynchronous operation that can return a value.
    /// </summary>
    /// <typeparam name="TResult">
    /// The type of the result produced by this <see cref="T:System.Threading.Tasks.Task" />. 
    /// </typeparam>
    public class Task<TResult> : Task
    {
        // Private fields
        private object func;
        private TResult result;
        private object futureState;

        /// <summary>
        /// Initializes a new <see cref="T:System.Threading.Tasks.Task`1" /> with the specified function.
        /// </summary>
        /// <param name="function">
        /// The delegate that represents the code to execute in the task. When the function has completed, 
        /// the task's <see cref="P:System.Threading.Tasks.Task`1.Result" /> property will be set to return 
        /// the result value of the function.
        /// </param>
        public Task(Func<TResult> function) : base(new Action<object>(InvokeFuture), null)
        {
            this.func = function;
            base.state = this;
        }

        public Task(Func<TResult> function, TaskCreationOptions creationOptions) : base(new Action<object>(InvokeFuture), null, creationOptions)
        {
            this.func = function;
            base.state = this;
        }

        public Task(Func<Task, TResult> function, object state) : base(new Action<object>(InvokeFuture), null)
        {
            this.func = function;
            base.state = this;
            this.futureState = state;
        }

        /// <summary>
        /// Gets the result value of this <see cref="T:System.Threading.Tasks.Task`1" />.
        /// </summary>
        /// <returns>
        /// The result value of this <see cref="T:System.Threading.Tasks.Task`1" />, which is the same type as the 
        /// task's type parameter.
        /// </returns>
        public TResult Result
        {
            get
            {
                //Wait for the task to complete
                this.Wait();

                //Return the result object
                return this.result;
            }
        }

        /// <summary>
        /// Implementation of the Task execution.
        /// </summary>
        /// <param name="futureAsObj">Task object.</param>
        private static void InvokeFuture(object futureAsObj)
        {
            Task<TResult> task = (Task<TResult>)futureAsObj;
            Func<TResult> valueSelector = task.func as Func<TResult>;

            if (valueSelector != null)
            {
                task.result = valueSelector();
            }

            else
            {
                task.result = ((Func<Task, TResult>)task.func)((Task) task.futureState);
            }
        }
    }
}

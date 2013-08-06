using System;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides support for creating and scheduling <see cref="T:System.Threading.Tasks.Task" /> objects.
    /// </summary>
    public class TaskFactory
    {
        /// <summary>
        /// Initializes a <see cref="T:System.Threading.Tasks.TaskFactory" /> instance with the default configuration.
        /// </summary>
        public TaskFactory()
        {

        }

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task" />.
        /// </summary>
        /// <param name="action">The action delegate to execute asynchronously.</param>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task" />.</returns>
        public Task StartNew(Action action)
        {
            return this.StartNew(action, TaskCreationOptions.None);
        }

        public Task StartNew(Action action, TaskCreationOptions creationOptions)
        {
            var task = new Task(action, creationOptions);
            task.Start();

            return task;
        }

        /// <summary>
        /// Creates and starts a <see cref="T:System.Threading.Tasks.Task{TResult}" />.
        /// </summary>
        /// <param name="function">
        /// A function delegate that returns the future result to be available through 
        /// the <see cref="T:System.Threading.Tasks.Task{TResult}" />.
        /// </param>
        /// <typeparam name="TResult">
        /// The type of the result available through the <see cref="T:System.Threading.Tasks.Task{TResult}" />.
        /// </typeparam>
        /// <returns>The started <see cref="T:System.Threading.Tasks.Task{TResult}" />.</returns>
        public Task<TResult> StartNew<TResult>(Func<TResult> function)
        {
            return this.StartNew<TResult>(function, TaskCreationOptions.None);
        }

        public Task<TResult> StartNew<TResult>(Func<TResult> function, TaskCreationOptions creationOptions)
        {
            var task = new Task<TResult>(function, creationOptions);
            task.Start();

            return task;
        }
    }
}

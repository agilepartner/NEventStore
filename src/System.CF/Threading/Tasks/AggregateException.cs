using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Threading.Tasks
{
    public class AggregateException : Exception
    {
        public AggregateException() : this("An aggregate exception has occured.", new Exception[0])
        {

        }

        public AggregateException(string message) : this(message, new Exception[0])
        {
   
        }

        public AggregateException(IEnumerable<Exception> innerExceptions) : this("An aggregate exception has occured.", innerExceptions)
        {

        }

        public AggregateException(string message, IEnumerable<Exception> innerExceptions) : this(message, innerExceptions != null ? new List<Exception>(innerExceptions) : new List<Exception>())
        {

        }

        public AggregateException(string message, Exception innerException) : this(message, new List<Exception>() { innerException })
        {

        }

        public AggregateException(string message, Exception[] innerExceptions) : this(message, (IEnumerable<Exception>)innerExceptions)
        {

        }

        private AggregateException(string message, IList<Exception> innerExceptions) : base(message, ((innerExceptions != null) && (innerExceptions.Count > 0)) ? innerExceptions[0] : null)
        {
            if (innerExceptions == null)
            {
                throw new ArgumentNullException("innerExceptions");
            }

            var list = new Exception[innerExceptions.Count];

            for (int i = 0; i < list.Length; i++)
            {
                list[i] = innerExceptions[i];

                if (list[i] == null)
                {
                    throw new ArgumentException("Inner exception cannot be null.");
                }
            }

            this.InnerExceptions = new ReadOnlyCollection<Exception>(list);
        }

        public ReadOnlyCollection<Exception> InnerExceptions
        {
            get;
            private set;
        }

        public override Exception GetBaseException()
        {
            if (this.InnerExceptions.Count > 0)
            {
                return this.InnerExceptions[0];
            }

            return null;
        }
    }
}

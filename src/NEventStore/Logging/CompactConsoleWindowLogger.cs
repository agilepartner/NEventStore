namespace NEventStore.Logging
{
    using System;

    public class ConsoleWindowLogger : ILog
    {
        private static readonly object Sync = new object();
        private readonly Type _typeToLog;

        public ConsoleWindowLogger(Type typeToLog)
        {
            _typeToLog = typeToLog;
        }

        public virtual void Verbose(string message, params object[] values)
        {
            Log("VERBOSE : ", message, values);
        }

        public virtual void Debug(string message, params object[] values)
        {
            Log("DEBUG : ", message, values);
        }

        public virtual void Info(string message, params object[] values)
        {
            Log("INFO :", message, values);
        }

        public virtual void Warn(string message, params object[] values)
        {
            Log("WARN : ", message, values);
        }

        public virtual void Error(string message, params object[] values)
        {
            Log("ERROR : ", message, values);
        }

        public virtual void Fatal(string message, params object[] values)
        {
            Log("FATAL : ", message, values);
        }

        private void Log(string type, string message, params object[] values)
        {
            lock (Sync)
            {
                Console.WriteLine(type + message.FormatMessage(_typeToLog, values));
            }
        }
    }
}
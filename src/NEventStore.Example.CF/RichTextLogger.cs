using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using NEventStore.Logging;
using System.Drawing;
using Resco.Controls.RichTextBox;

namespace NEventStore.Example.CF
{
    public class RichTextLogger : ILog
    {
        private static readonly object Sync = new object();
        private readonly Color _originalColor;
        private readonly Type _typeToLog;
        private readonly RichTextBox _log;

        public RichTextLogger(Type typeToLog, RichTextBox log)
        {
            _typeToLog = typeToLog;
            _log = log;
            _originalColor = log.ForeColor;
        }

        public virtual void Verbose(string message, params object[] values)
        {
            //Log(Color.DarkGreen, message, values);
        }

        public virtual void Debug(string message, params object[] values)
        {
            //Log(Color.Green, message, values);
        }

        public virtual void Info(string message, params object[] values)
        {
            Log(Color.White, message, values);
        }

        public virtual void Warn(string message, params object[] values)
        {
            Log(Color.Yellow, message, values);
        }

        public virtual void Error(string message, params object[] values)
        {
            Log(Color.DarkRed, message, values);
        }

        public virtual void Fatal(string message, params object[] values)
        {
            Log(Color.Red, message, values);
        }

        private void Log(Color color, string message, params object[] values)
        {
            lock (Sync)
            {
                _log.ForeColor = color;
                _log.AppendText(message.FormatMessage(this.GetType(), values));
                _log.ForeColor = _originalColor;
            }
        }

    }
}

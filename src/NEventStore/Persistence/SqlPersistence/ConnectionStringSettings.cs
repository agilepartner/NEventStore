using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NEventStore.Persistence.SqlPersistence
{
    public class ConnectionStringSettings
    {
        private readonly string _path;
        private readonly string _password;

        public ConnectionStringSettings(string path)
            : this(path, null)
        {
        }

        public ConnectionStringSettings(string path, string password)
        {
            _path = PathHelper.GetFullPath(path);
            _password = password;
        }

        public string Name { get { return _path; } }

        public string ConnectionString
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Data Source=");
                builder.Append(_path);
                if (!String.IsNullOrEmpty(_password)) 
                {
                    builder.Append(";Password=");
                    builder.Append(_password);
                }

                return builder.ToString();
            }
        }

    }
}

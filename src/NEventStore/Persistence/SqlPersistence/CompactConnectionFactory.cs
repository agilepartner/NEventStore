using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using NEventStore.Logging;
using System.Globalization;

namespace NEventStore.Persistence.SqlPersistence
{
    public class SQLiteConnectionFactory : CompactConnectionFactory
    {
        private const string DefaultDatabaseName = "EventStore.db";

        public SQLiteConnectionFactory()
            : base(new ConnectionStringSettings(DefaultDatabaseName))
        {
        }

        public SQLiteConnectionFactory(ConnectionStringSettings master)
            : base(master, master, DefaultShards)
        {
        }

        public SQLiteConnectionFactory(ConnectionStringSettings master, ConnectionStringSettings replica, int shards) 
            : base(master, replica, shards)
        {
        }

        protected override IDbConnection CreateConnection(string connectionString)
        {
            return new System.Data.SQLite.SQLiteConnection(connectionString);
        }
    }
    public class SqlCeConnectionFactory : CompactConnectionFactory
    {
        private const string DefaultDatabaseName = "EventStore.sdf";


        public SqlCeConnectionFactory()
            : base(new ConnectionStringSettings(DefaultDatabaseName))
        {
        }

        public SqlCeConnectionFactory(ConnectionStringSettings master)
            : base(master, master, DefaultShards)
        {
        }

        public SqlCeConnectionFactory(ConnectionStringSettings master, ConnectionStringSettings replica, int shards) 
            : base(master, replica, shards)
        {
        }
        
        protected override IDbConnection CreateConnection(string connectionString)
        {
            return new System.Data.SqlServerCe.SqlCeConnection(connectionString);
        }
    }
    
    public abstract class CompactConnectionFactory : IConnectionFactory
    {
        protected const int DefaultShards = 16;

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SqlCeConnectionFactory));

        private readonly ConnectionStringSettings _master;
        private readonly ConnectionStringSettings _replica;
        private readonly int _shards;

        public CompactConnectionFactory(ConnectionStringSettings master)
            : this(master, master, DefaultShards)
        {
        }

        public CompactConnectionFactory(ConnectionStringSettings master, ConnectionStringSettings replica, int shards)
        {
            if (master == null)
                throw new ArgumentNullException("master");

            _master = master;
            _replica = replica;
            _shards = shards >= 0 ? shards : DefaultShards;

            Logger.Debug(Messages.ConfiguringConnections,
                _master.Name, _master.Name, _shards);
        }

        public IDbConnection OpenMaster(Guid streamId)
        {
            Logger.Verbose(Messages.OpeningMasterConnection, _master.Name);
            return Open(streamId, _master);
        }

        public IDbConnection OpenReplica(Guid streamId)
        {
            Logger.Verbose(Messages.OpeningReplicaConnection, _replica.Name);
            return Open(streamId, _replica);
        }

        private IDbConnection Open(Guid streamId, ConnectionStringSettings settings)
        {
            var connectionString = BuildConnectionString(streamId, settings);

            return new ConnectionScope(connectionString, () => OpenConnection(connectionString));
        }

        private IDbConnection OpenConnection(string connectionString)
        {
            var connection = CreateConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                throw new StorageUnavailableException(e.Message, e);
            }
            return connection;
        }

        protected abstract IDbConnection CreateConnection(string connectionString);

        protected virtual string BuildConnectionString(Guid streamId, ConnectionStringSettings setting)
        {
            if (_shards == 0)
            {
                return setting.ConnectionString;
            }

            Logger.Verbose(Messages.EmbeddingShardKey, setting.Name);
            return setting.ConnectionString.FormatWith(ComputeHashKey(streamId));
        }

        protected virtual string ComputeHashKey(Guid streamId)
        {
            // simple sharding scheme which could easily be improved through such techniques
            // as consistent hashing (Amazon Dynamo) or other kinds of sharding.
            return (_shards == 0 ? 0 : streamId.ToByteArray()[0] % _shards).ToString(CultureInfo.InvariantCulture);
        }
    }
}

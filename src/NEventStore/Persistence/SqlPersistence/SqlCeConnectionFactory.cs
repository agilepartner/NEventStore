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
    public class SqlCeConnectionFactory : IConnectionFactory
    {
        private const int DefaultShards = 16;
        private const string DefaultConnectionName = "EventStore";

        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(SqlCeConnectionFactory));

        private readonly ConnectionStringSettings _masterSettings;
        private readonly ConnectionStringSettings _replicaSettings;
        private readonly int _shards;

        public SqlCeConnectionFactory()
            : this(new ConnectionStringSettings(DefaultConnectionName))
        {
        }

        public SqlCeConnectionFactory(ConnectionStringSettings masterSettings)
            : this(masterSettings, masterSettings, DefaultShards)
        {
        }

        public SqlCeConnectionFactory(ConnectionStringSettings masterSettings, ConnectionStringSettings replicaSettings, int shards)
        {
            if (masterSettings == null)
                throw new ArgumentNullException("masterSettings");

            _masterSettings = masterSettings;
            _replicaSettings = replicaSettings;
            _shards = shards >= 0 ? shards : DefaultShards;

            Logger.Debug(Messages.ConfiguringConnections,
                _masterSettings.Name, _masterSettings.Name, _shards);
        }

        public IDbConnection OpenMaster(Guid streamId)
        {
            Logger.Verbose(Messages.OpeningMasterConnection, _masterSettings.Name);
            return Open(streamId, _masterSettings);
        }

        public IDbConnection OpenReplica(Guid streamId)
        {
            Logger.Verbose(Messages.OpeningReplicaConnection, _replicaSettings.Name);
            return Open(streamId, _replicaSettings);
        }

        private IDbConnection Open(Guid streamId, ConnectionStringSettings settings)
        {
            var connectionString = BuildConnectionString(streamId, settings);

            return new ConnectionScope(connectionString, () => OpenConnection(connectionString));
        }

        private static IDbConnection OpenConnection(string connectionString)
        {
            var connection = new System.Data.SqlServerCe.SqlCeConnection(connectionString);
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

namespace NEventStore
{
    using NEventStore.Persistence.SqlPersistence;

    public static class SqlPersistenceWireupExtensions
    {
#if PocketPC
        public static SqlPersistenceWireup UsingSqlCePersistence(this Wireup wireup, ConnectionStringSettings master) 
        {
            var factory = new SqlCeConnectionFactory(master, master, 0);
            return wireup.UsingSqlPersistence(factory);
        }
#else
        public static SqlPersistenceWireup UsingSqlPersistence(this Wireup wireup, string connectionName)
        {
            var factory = new ConfigurationConnectionFactory(connectionName);
            return wireup.UsingSqlPersistence(factory);
        }

        public static SqlPersistenceWireup UsingSqlPersistence(
            this Wireup wireup, string masterConnectionName, string replicaConnectionName)
        {
            var factory = new ConfigurationConnectionFactory(masterConnectionName, replicaConnectionName, 1);
            return wireup.UsingSqlPersistence(factory);
        }
#endif



        public static SqlPersistenceWireup UsingSqlPersistence(this Wireup wireup, IConnectionFactory factory)
        {
            return new SqlPersistenceWireup(wireup, factory);
        }
    }
}
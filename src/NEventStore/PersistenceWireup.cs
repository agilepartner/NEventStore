namespace NEventStore
{
    using System;
    using System.Transactions;
#if !PocketPC
    using NEventStore.Diagnostics; 
#endif
    using NEventStore.Logging;
    using NEventStore.Persistence;
    using NEventStore.Serialization;

    public class PersistenceWireup : Wireup
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (PersistenceWireup));
        private bool _initialize;
#if !PocketPC
        private bool _tracking;
        private string _trackingInstanceName;
#endif

        public PersistenceWireup(Wireup inner)
            : base(inner)
        {
            Container.Register(TransactionScopeOption.Suppress);
        }

        public virtual PersistenceWireup WithPersistence(IPersistStreams instance)
        {
            Logger.Debug(Messages.RegisteringPersistenceEngine, instance.GetType());
            With(instance);
            return this;
        }

        protected virtual SerializationWireup WithSerializer(ISerialize serializer)
        {
            return new SerializationWireup(this, serializer);
        }

        public virtual PersistenceWireup InitializeStorageEngine()
        {
            Logger.Debug(Messages.ConfiguringEngineInitialization);
            _initialize = true;
            return this;
        }

#if !PocketPC
        public virtual PersistenceWireup TrackPerformanceInstance(string instanceName)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException("instanceName", Messages.InstanceCannotBeNull);
            }

            Logger.Debug(Messages.ConfiguringEnginePerformanceTracking);
            _tracking = true;
            _trackingInstanceName = instanceName;
            return this;
        }
#endif
        
        public virtual PersistenceWireup EnlistInAmbientTransaction()
        {
            Logger.Debug(Messages.ConfiguringEngineEnlistment);
            Container.Register(TransactionScopeOption.Required);
            return this;
        }

        public override IStoreEvents Build()
        {
            Logger.Debug(Messages.BuildingEngine);
            var engine = Container.Resolve<IPersistStreams>();

            if (_initialize)
            {
                Logger.Debug(Messages.InitializingEngine);
                engine.Initialize();
            }

#if !PocketPC
            if (_tracking)
            {
                Container.Register<IPersistStreams>(new PerformanceCounterPersistenceEngine(engine, _trackingInstanceName));
            } 
#endif

            return base.Build();
        }
    }
}
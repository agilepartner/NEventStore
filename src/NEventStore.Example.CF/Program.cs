using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using NEventStore.Persistence.SqlPersistence;
using NEventStore.Persistence.SqlPersistence.SqlDialects;
using NEventStore.Dispatcher;
using System.IO;

namespace NEventStore.Example.CF
{
    class Program
    {
        private static readonly Guid StreamId = Guid.NewGuid(); // aggregate identifier
        private static readonly byte[] EncryptionKey = new byte[]
		{
			0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf
		}; 
        private static IStoreEvents store;
        private static ConnectionStringSettings _connectionSettings;
        private static readonly string StorageCard = @"\Storage Card";
        private static readonly string EventStore = "EventStore.sdf";

        static void Main(string[] args)
        {
            _connectionSettings = new ConnectionStringSettings(EventStore);

            using (var scope = new TransactionScope())
            using (store = WireupEventStore())
            {
                OpenOrCreateStream();
                AppendToStream();
                TakeSnapshot();
                LoadFromSnapshotForwardAndAppend();
                scope.Complete();

            }

            if (Directory.Exists(StorageCard) && File.Exists(_connectionSettings.Name))
            {
                Console.WriteLine("Copy file {0} to storage card", _connectionSettings.Name);
                File.Copy(_connectionSettings.Name, Path.Combine(StorageCard, EventStore), true);
            }

            Console.WriteLine(Resources.PressAnyKey);
            Console.ReadLine();
        }

        private static IStoreEvents WireupEventStore()
        {
            return Wireup.Init()
               //.LogToOutputWindow()
               .UsingInMemoryPersistence()
               .UsingSqlCePersistence(_connectionSettings)
                   .WithDialect(new SqlCeDialect())
                   .EnlistInAmbientTransaction() // two-phase commit
                   .InitializeStorageEngine()
                   //.TrackPerformanceInstance("example")
                   .UsingJsonSerialization()
                       .Compress() 
                       .EncryptWith(EncryptionKey)
               .HookIntoPipelineUsing(new[] { new PipelineHook() })
               .UsingSynchronousDispatchScheduler()
                   .DispatchTo(new DelegateMessageDispatcher(DispatchCommit))
               .Build();
        }

        private static void DispatchCommit(Commit commit)
        {
            // This is where we'd hook into our messaging infrastructure, such as NServiceBus,
            // MassTransit, WCF, or some other communications infrastructure.
            // This can be a class as well--just implement IDispatchCommits.
            try
            {
                foreach (var @event in commit.Events)
                    Console.WriteLine(Resources.MessagesDispatched + (@event.Body.ToString()));
            }
            catch (Exception)
            {
                Console.WriteLine(Resources.UnableToDispatch);
            }
        }

        private static void OpenOrCreateStream()
        {
            // we can call CreateStream(StreamId) if we know there isn't going to be any data.
            // or we can call OpenStream(StreamId, 0, int.MaxValue) to read all commits,
            // if no commits exist then it creates a new stream for us.
            using (var stream = store.OpenStream(StreamId, 0, int.MaxValue))
            {
                var @event = new TourCreated(StreamId, "BT121", "BT121", "DEV123");

                stream.Add(new EventMessage { Body = @event });
                stream.CommitChanges(Guid.NewGuid());
            }
        }

        private static void AppendToStream()
        {
            using (var stream = store.OpenStream(StreamId, int.MinValue, int.MaxValue))
            {
                var event1 = new TourStarted(StreamId);
                var event2 = new TourSuspended(StreamId);
                var event3 = new TourStarted(StreamId);

                stream.Add(new EventMessage { Body = event1 });
                stream.Add(new EventMessage { Body = event2 });
                stream.Add(new EventMessage { Body = event3 });
                stream.CommitChanges(Guid.NewGuid());
            }
        }

        private static void TakeSnapshot()
        {
            var memento = new AggregateMemento { Value = "snapshot" };
            store.Advanced.AddSnapshot(new Snapshot(StreamId, 2, memento));
        }

        private static void LoadFromSnapshotForwardAndAppend()
        {
            var latestSnapshot = store.Advanced.GetSnapshot(StreamId, int.MaxValue);

            using (var stream = store.OpenStream(latestSnapshot, int.MaxValue))
            {
                var event4 = new TourFinished(StreamId);

                stream.Add(new EventMessage { Body = event4 });
                stream.CommitChanges(Guid.NewGuid());
            }
        }
    }
}

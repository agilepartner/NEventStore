namespace NEventStore.Example
{
    using System;
    using System.Transactions;
    using NEventStore;
    using NEventStore.Dispatcher;
    using NEventStore.Persistence.SqlPersistence.SqlDialects;

    internal static class MainProgram
	{
		private static readonly byte[] EncryptionKey = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf };
		private static readonly Random _random = new Random();

		private static Guid StreamId;
		private static IStoreEvents store;

		private static void Main()
		{
			using (store = WireupEventStore())
			{
				CreateStream();

				ConsoleKeyInfo key;
				do {
					Display();
					key = Console.ReadKey();
					switch(key.Key) {
						case ConsoleKey.C:
							Console.WriteLine("\nCreating new stream");
							CreateStream();
							break;
						case ConsoleKey.A:
							int numberOfEvents = _random.Next(10);
							Console.WriteLine("\nAppending {0} events to stream", numberOfEvents);
							AppendToStream(numberOfEvents);
							break;
						case ConsoleKey.S:
							Console.Write("\nTaking snapshot...");
							TakeSnapshot();
							Console.WriteLine(" done");
							break;
						case ConsoleKey.L:
							Console.Write("\nLoading snapshot...");
							LoadFromSnapshotForwardAndAppend();
							Console.WriteLine(" done");
							break;
						case ConsoleKey.Q:
							Console.WriteLine("\nQuitting...");
							break;
					}

				} while(key.Key != ConsoleKey.Q);
			}
		}

		private static void Display() {
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("C : Create new stream");
			Console.WriteLine("A : Append events to stream");
			Console.WriteLine("S : Take snapshot");
			Console.WriteLine("L : Take snapshot");
			Console.WriteLine("Q : quit");
		}

		private static IStoreEvents WireupEventStore()
		{
			 return Wireup.Init()
				.LogToOutputWindow()
				.UsingInMemoryPersistence()
				.UsingSqlPersistence("EventStore") // Connection string is in app.config
					.WithDialect(new MsSqlDialect())
					.EnlistInAmbientTransaction() // two-phase commit
					.InitializeStorageEngine()
					.TrackPerformanceInstance("example")
					.UsingJsonSerialization()
						.Compress()
						.EncryptWith(EncryptionKey)
				.HookIntoPipelineUsing(new[] { new AuthorizationPipelineHook() })
				.UsingTimerDispatchScheduler(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))
					.DispatchTo(new DelegateMessageDispatcher(DispatchCommit))
				.Build();
		}
		private static bool DispatchCommit(Commit commit)
		{
			// This is where we'd hook into our messaging infrastructure, such as NServiceBus,
			// MassTransit, WCF, or some other communications infrastructure.
			// This can be a class as well--just implement IDispatchCommits.
			try
			{
				foreach (var @event in commit.Events)
					Console.WriteLine(Resources.MessagesDispatched + ((SomeDomainEvent)@event.Body).Value);
				return true;
			}
			catch (Exception)
			{
				Console.WriteLine(Resources.UnableToDispatch);
				return false;
			}

		}

		private static void CreateStream()
		{
			// we can call CreateStream(StreamId) if we know there isn't going to be any data.
			// or we can call OpenStream(StreamId, 0, int.MaxValue) to read all commits,
			// if no commits exist then it creates a new stream for us.
			StreamId = Guid.NewGuid();

			using (var stream = store.OpenStream(StreamId, 0, int.MaxValue))
			{
				var @event = new SomeDomainEvent("Initial event");

				stream.Add(new EventMessage { Body = @event });
				stream.CommitChanges(Guid.NewGuid());

				Console.WriteLine("Stream created {0}", StreamId);
			}
		}
		private static void AppendToStream(int number)
		{
			using (var stream = store.OpenStream(StreamId, 0, int.MaxValue))
			{
				for(int i = 0 ; i < number ; i++)
				{
					var value = String.Format("Event #{0}.", stream.StreamRevision + i);
					Console.WriteLine(value);
					var @event = new SomeDomainEvent(value);

					stream.Add(new EventMessage { Body = @event });
				}

				stream.CommitChanges(Guid.NewGuid());
			}
		}
		private static void TakeSnapshot()
		{
			using(var stream = store.OpenStream(StreamId, int.MinValue, int.MaxValue))
			{
				var @event = new SomeDomainEvent("Snapshot taken");

				stream.Add(new EventMessage { Body = @event });
				stream.CommitChanges(Guid.NewGuid());

				var memento = new AggregateMemento { Value = "snapshot" };
				store.Advanced.AddSnapshot(new Snapshot(StreamId, stream.StreamRevision, memento));
			}
		}
		private static void LoadFromSnapshotForwardAndAppend()
		{
			var latestSnapshot = store.Advanced.GetSnapshot(StreamId, int.MaxValue);

			using (var stream = store.OpenStream(latestSnapshot, int.MaxValue))
			{
				var @event = new SomeDomainEvent("Loaded from snapshot");

				stream.Add(new EventMessage { Body = @event });
				stream.CommitChanges(Guid.NewGuid());
			}
		}
	}
}
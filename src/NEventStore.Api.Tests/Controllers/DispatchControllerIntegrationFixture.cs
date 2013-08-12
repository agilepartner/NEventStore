using System;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Api.Controllers;
using NEventStore.Dispatcher;
using NUnit.Framework;

namespace NEventStore.Api.Tests.Controllers
{
	[TestFixture]
	public class DispatchControllerIntegrationFixture
	{
		private static readonly string EventBody = "This is an awesome event";
		private static readonly string HeaderName = "TestHeader";
		private static readonly string OtherHeaderName = "OtherTestHeader";
		private static readonly string HeaderContent = "TestHeaderValue";

		private static readonly Guid StreamId = Guid.NewGuid();
		private static readonly Guid CommitId = Guid.NewGuid();
		private static readonly DateTime CommitStamp = new DateTime(2013, 08, 12);
		private static readonly Dictionary<string, object> Headers = new Dictionary<string, object> { { HeaderName, HeaderContent } };
		private static readonly List<EventMessage> Events = new List<EventMessage> { new EventMessage { Body = EventBody } };

		private static int StreamRevision = 0;
		private static int CommitSequence = 0;
		private static bool IsCommitted = false;
		private static bool IsDispatched = false;

		[Test]
		public void PostFirstCommit()
		{
			using(IStoreEvents store = WireUpStore())
			{
				// Arrange
				DispatchController controller = new DispatchController(store);
				StreamRevision = 1;
				CommitSequence = 1;
				IsCommitted = false;
				IsDispatched = false;

				Commit commit = new Commit(StreamId, StreamRevision, CommitId, CommitSequence, CommitStamp, Headers, Events);

				// Act
				controller.Post(commit);

				// Assert
				IEventStream stream = store.OpenStream(StreamId, 0, int.MaxValue);
				Assert.That(stream, Is.Not.Null);
				stream.AssertCommit(commit);
				stream.AssertDispatched(true);

				Assert.That(IsCommitted, Is.True);
				Assert.That(IsDispatched, Is.True);
			}
		}

		[Test]
		public void PostSecondCommit()
		{
			PrepareStreamWithEvents(2);

			using(IStoreEvents store = WireUpStore())
			{
				// Arrange
				DispatchController controller = new DispatchController(store);
				StreamRevision = 3;
				CommitSequence = 2;
				IsCommitted = false;
				IsDispatched = false;

				Commit commit = new Commit(StreamId, StreamRevision, CommitId, CommitSequence, CommitStamp, Headers, Events);

				// Act
				controller.Post(commit);

				// Assert
				IEventStream stream = store.OpenStream(StreamId, 0, int.MaxValue);
				Assert.That(stream, Is.Not.Null);
				Assert.That(stream.CommittedHeaders.ContainsKey(OtherHeaderName), Is.True);
				stream.AssertCommit(commit);
				stream.AssertDispatched(true);

				Assert.That(IsCommitted, Is.True);
				Assert.That(IsDispatched, Is.True);
			}
		}

		private void PrepareStreamWithEvents(int numberOfEvents)
		{
			using(IStoreEvents store = WireUpStore(false))
			{
				IEventStream stream = store.OpenStream(StreamId, 0, int.MaxValue);
				for(int i = 0 ; i < numberOfEvents ; i++)
				{
					stream.Add(new EventMessage { Body = "Event " + (i + 1) });
				}
				stream.UncommittedHeaders.Add(OtherHeaderName, Guid.NewGuid()); 
				stream.CommitChanges(Guid.NewGuid());
			}
		}

		private IStoreEvents WireUpStore(bool testing = true)
		{
			var wireup =  Wireup.Init()
				.UsingSqlPersistence("EventStore")
					.WithDialect(new NEventStore.Persistence.SqlPersistence.SqlDialects.SqlCeDialect())
					.InitializeStorageEngine()
					.UsingBinarySerialization();

			if (testing)
			{
				wireup.HookIntoPipelineUsing(new[] { new TestKook() })
				.UsingSynchronousDispatchScheduler()
				.DispatchTo(new DelegateMessageDispatcher(DispatchTo));
			}

			return wireup.Build();
		}

		private bool DispatchTo(Commit commit)
		{
			return IsDispatched = true;
		}

		private static void AssertEvents(ICollection<EventMessage> events)
		{
			Assert.That(events.Count, Is.EqualTo(1));
			Assert.That(events.First().Body, Is.EqualTo(EventBody));
		}

		private static void AssertHeaders(IDictionary<string, object> headers)
		{
			Assert.That(headers.Count, Is.EqualTo(1));
			Assert.That(headers.First().Key, Is.EqualTo(HeaderName));
			Assert.That(headers.First().Value, Is.EqualTo(HeaderContent));
		}

		private class TestKook : IPipelineHook
		{
			public Commit Select(Commit committed)
			{
				return committed;
			}

			public bool PreCommit(Commit attempt)
			{
				return true;
			}

			public void PostCommit(Commit committed)
			{
				IsCommitted = true;
			}

			public void Dispose()
			{
			}
		}
	}
}

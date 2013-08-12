using System;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Api.Controllers;
using NUnit.Framework;

namespace NEventStore.Api.Tests.Controllers
{
	[TestFixture]
	public class CommitControllerIntegrationFixture
	{
		private static readonly string EventBody = "This is an awesome event";
		private static readonly string HeaderName = "TestHeader";
		private static readonly string HeaderContent = "TestHeaderValue";

		private	static readonly Guid StreamId = Guid.NewGuid();
		private static readonly int StreamRevision = 25;
		private static readonly Guid CommitId = Guid.NewGuid();
		private static readonly int CommitSequence = 10;
		private static readonly DateTime CommitStamp = new DateTime(2013, 08,12);
		private static readonly Dictionary<string, object> Headers = new Dictionary<string, object> { { HeaderName, HeaderContent } };
		private static readonly List<EventMessage> Events = new List<EventMessage> { new EventMessage{ Body = EventBody } };

		[Test]
		public void Post()
		{
			using (IStoreEvents store = WireUpStore())
			{
				// Arrange
				CommitController controller = new CommitController(store);

				Commit commit = new Commit(StreamId, StreamRevision, CommitId, CommitSequence, CommitStamp, Headers, Events);

				// Act
				controller.Post(commit);

				// Assert
				IEventStream stream = store.OpenStream(StreamId, 0, int.MaxValue);
				Assert.That(stream, Is.Not.Null);
				stream.AssertDispatched(true);
			}
		}

		private IStoreEvents WireUpStore() 
		{
			return Wireup.Init()
				.UsingSqlPersistence("EventStore")
					.WithDialect(new NEventStore.Persistence.SqlPersistence.SqlDialects.SqlCeDialect())
					.InitializeStorageEngine()
					.UsingBinarySerialization()
					.HookIntoPipelineUsing(new [] { new TestKook()  })
				.Build();
		}

		private static void AssertCommit(Commit committed)
		{
			Assert.That(committed, Is.Not.Null);
			Assert.That(committed.StreamId, Is.EqualTo(StreamId));
			Assert.That(committed.StreamRevision, Is.EqualTo(StreamRevision));
			Assert.That(committed.CommitId, Is.EqualTo(CommitId));
			Assert.That(committed.CommitSequence, Is.EqualTo(CommitSequence));
			Assert.That(committed.CommitStamp, Is.EqualTo(CommitStamp));
			Assert.That(committed.Events.Count, Is.EqualTo(1));
			Assert.That(committed.Events.First().Body, Is.EqualTo(EventBody));
			Assert.That(committed.Headers.Count, Is.EqualTo(1));
			Assert.That(committed.Headers.First().Key, Is.EqualTo(HeaderName));
			Assert.That(committed.Headers.First().Value, Is.EqualTo(HeaderContent));
		}

		private class TestKook : IPipelineHook
		{
			public Commit Select(Commit committed)
			{
				AssertCommit(committed);
				return committed;
			}

			public bool PreCommit(Commit attempt)
			{
				return true;
			}

			public void PostCommit(Commit committed)
			{
			}

			public void Dispose()
			{
			}
		}
	}


}

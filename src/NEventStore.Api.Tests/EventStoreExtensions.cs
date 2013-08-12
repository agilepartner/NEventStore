using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NEventStore.Api.Tests
{
	public static class EventStoreExtensions
	{
		public static void AssertDispatched(this IEventStream stream, bool value) 
		{
			var sql = @"SELECT Dispatched 
						FROM Commits 
						WHERE StreamId = @StreamId
						AND CommitSequence = @CommitSequence;";
			using(SqlCeConnection connection = new SqlCeConnection("Data Source=EventStore.sdf"))
			{
				connection.Open();
				var cmd = connection.CreateCommand();
				cmd.CommandText = sql;
				cmd.Parameters.AddWithValue("StreamId", stream.StreamId);
				cmd.Parameters.AddWithValue("CommitSequence", stream.CommitSequence);
				bool dispatched = (bool) cmd.ExecuteScalar();
				Assert.That(dispatched, Is.True);
			}
		}

		public static void AssertCommit(this IEventStream stream, Commit commit) 
		{
			Assert.That(commit, Is.Not.Null);
			Assert.That(stream.StreamId, Is.EqualTo(commit.StreamId));
			Assert.That(stream.StreamRevision, Is.EqualTo(commit.StreamRevision));
			Assert.That(stream.CommitSequence, Is.EqualTo(commit.CommitSequence));

			foreach(var item in commit.Headers)
			{
				Assert.That(stream.CommittedHeaders.ContainsKey(item.Key), Is.True);
				Assert.That(item.Value, Is.EqualTo(stream.CommittedHeaders[item.Key]));
			}
			foreach(var evt in commit.Events)
			{
				var committedEvent = stream.CommittedEvents.SingleOrDefault(e => ((string)e.Body)  == ((string)evt.Body));
				Assert.That(committedEvent, Is.Not.Null);
			}
		}

	}
}

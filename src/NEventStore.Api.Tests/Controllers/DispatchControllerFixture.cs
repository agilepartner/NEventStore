using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NEventStore.Api.Controllers;
using NSubstitute;
using NUnit.Framework;

namespace NEventStore.Api.Tests.Controllers
{
	[TestFixture]
	public class DispatchControllerFixture
	{
		private static readonly Guid StreamId = Guid.NewGuid();
		private static readonly Guid CommitId = Guid.NewGuid();

		[Test]
		public void Post()
		{
			// Arrange
			IStoreEvents store = Substitute.For<IStoreEvents>();
			IEventStream stream = Substitute.For<IEventStream>();
			store.OpenStream(StreamId, Arg.Any<int>(), Arg.Any<int>()).Returns(c => stream);

			DispatchController controller = new DispatchController(store);

			Commit commit = new Commit(StreamId, 1, CommitId, 1, DateTime.Now, 
				new Dictionary<string, object>{ { "Key", "Value" } }, 
				new List<EventMessage> { new EventMessage(), new EventMessage() });

			// Act
			controller.Post(commit);

			// Assert
			store.Received().OpenStream(StreamId, Arg.Any<int>(), Arg.Any<int>());
			stream.Received(2).Add(Arg.Any<EventMessage>());
			stream.UncommittedHeaders.Received()["Key"] = "Value";
			stream.Received().CommitChanges(CommitId);
		}

	}
}

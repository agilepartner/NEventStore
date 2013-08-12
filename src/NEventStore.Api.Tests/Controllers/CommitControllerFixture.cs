using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using NEventStore;
using NEventStore.Api;
using NEventStore.Api.Controllers;
using NSubstitute;
using NUnit.Framework;

namespace NEventStore.Api.Tests.Controllers
{
	[TestFixture]
	public class CommitControllerFixture
	{

		[Test]
		public void Post()
		{
			// Arrange
			IStoreEvents store = Substitute.For<IStoreEvents>();
			CommitController controller = new CommitController(store);

			Commit commit = new Commit(Guid.NewGuid(), 1, Guid.NewGuid(), 1, DateTime.Now, new Dictionary<string, object>(), new List<EventMessage> { new EventMessage() });

			// Act
			controller.Post(commit);

			// Assert
			store.Advanced.Received().Commit(commit);

		}


	}

}

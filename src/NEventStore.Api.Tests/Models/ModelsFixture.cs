using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NEventStore.Api.Models;
using NSubstitute;
using NUnit.Framework;

namespace NEventStore.Api.Tests.Models
{
	[TestFixture]
	public class ModelsFixture
	{
		private static readonly Guid StreamId = Guid.NewGuid();

		[Test]
		public void ConstructEvent()
		{
			IEventStream stream = Substitute.For<IEventStream>();
			stream.StreamId.Returns(StreamId);
			EventMessage eventMessage = CreateEventMessage();

			Event evt = new Event(stream, eventMessage, 2);

			Assert.That(evt.Id, Is.EqualTo(2));
			Assert.That(evt.Content, Is.Not.Null.Or.Empty);
			Assert.That(evt.Title, Is.StringStarting("2"));
			Assert.That(evt.Title, Is.StringEnding(StreamId.ToString()));
			Assert.That(evt.Summary, Is.EqualTo((typeof(EventContent).Name)));
			Assert.That(evt.Tags.Length, Is.EqualTo(1));
			Assert.That(evt.Tags[0], Is.EqualTo("Header=Value"));
		}

		private static EventMessage CreateEventMessage()
		{
			EventMessage eventMessage = new EventMessage();
			eventMessage.Body = new EventContent("123", "ABC");
			eventMessage.Headers.Add("Header", "Value");
			return eventMessage;
		}

		private class EventContent
		{
			public EventContent(string code, string name)
			{
				Code = code;
				Name = name;
			}

			public readonly string Code;
			public readonly string Name;
		}

		[Test]
		public void ConstructStream()
		{
			IEventStream eventStream = Substitute.For<IEventStream>();
			eventStream.StreamId.Returns(StreamId);
			eventStream.StreamRevision.Returns(15);
			eventStream.CommitSequence.Returns(30);
			eventStream.CommittedHeaders.Returns(new Dictionary<string, object> { { "Header", "Value" } });
			eventStream.CommittedEvents.Returns(new List<EventMessage> { CreateEventMessage(), CreateEventMessage() });

			Stream stream = new Stream(eventStream);

			Assert.That(stream.Id, Is.EqualTo(StreamId));
			Assert.That(stream.Title, Is.StringContaining(StreamId.ToString()));
			Assert.That(stream.Summary, Is.EqualTo("StreamRevision=15;CommitSequence=30"));
			Assert.That(stream.Events.Count(), Is.EqualTo(2));
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NEventStore.Api.Syndication.Atom.AtomPub;
using NEventStore.Api.Syndication.Atom.Links;

namespace NEventStore.Api.Models
{
	public class Stream : Resource, IPublicationFeed	{

		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Summary { get; set; }
		public string Author { get; set; }
		public IEnumerable<Event> Events { get; set; }

		IEnumerable<IPublication> IPublicationFeed.Items
		{
			get { return Events; }
		}

		public Stream(IEventStream stream)
		{
			this.Id = stream.StreamId;
			this.Title = String.Format("{0} '{1}'", stream.CommittedHeaders[Headers.AggregateTypeHeader], stream.StreamId);
			this.Summary = String.Format("StreamRevision={0};CommitSequence={1}", stream.StreamRevision, stream.CommitSequence);
			this.Events = BuildEvents(stream).ToList();
		}

		private IEnumerable<Event> BuildEvents(IEventStream stream)
		{
			int i = 1;
			foreach (var item in stream.CommittedEvents)
			{
				yield return new Event(stream, item, i++);
			}
		}

	
	}
}
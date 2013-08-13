using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NEventStore.Api.Syndication.Atom.AtomPub;
using NEventStore.Api.Syndication.Atom.Links;

namespace NEventStore.Api.Models
{
	public class StreamFeed : Resource, IPublicationFeed	{

		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Summary { get; set; }
		public string Author { get; set; }
		public IEnumerable<Event> Events { get; set; }

		IEnumerable<IPublication> IPublicationFeed.Items
		{
			get { return Events; }
		}

		public StreamFeed(IEventStream stream)
		{
			this.Id = stream.StreamId;
			this.Title = stream.GetTitle();
			this.Summary = stream.GetSummary();
			this.Events = BuildEvents(stream).ToList();
			this.Author = FeedValues.Author;
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
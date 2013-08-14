using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NEventStore.Api.Syndication.Atom.AtomPub;
using NEventStore.Api.Syndication.Atom.Links;

namespace NEventStore.Api.Models
{
	public class StreamFeed : Resource, IPublicationFeed	{

		public Guid Id { get; private set; }
		public string Title { get; private set; }
		public string Summary { get; private set; }
		public string Author { get; private set; }
		public int From { get; private set; }
		public int Take { get; private set; }

		public IEnumerable<Event> Events { get; set; }

		private StreamFeed()
		{
			this.Author = FeedValues.Author;
		}

		public static StreamFeed EmptyStream(IEventStream stream, int from, int take) 
		{
			var feed = new StreamFeed();
			feed.Id = stream.StreamId;
			feed.Title = stream.GetTitle();
			feed.Summary = stream.GetSummary();
			feed.From = from;
			feed.Take = take;
			feed.Events = new List<Event>();

			return feed;
		}

		public static StreamFeed FromStream(IEventStream stream, int from, int take)
		{
			var feed = new StreamFeed();
			feed.Id = stream.StreamId;
			feed.Title = stream.GetTitle();
			feed.Summary = stream.GetSummary();
			feed.From = from;
			feed.Take = take;

			feed.Events = BuildEvents(stream, from);

			return feed;
		}

		private static IEnumerable<Event> BuildEvents(IEventStream stream, int from)
		{
			int i = from;
			return stream.CommittedEvents.Select(e => Event.FromStream(stream, e, i++)).ToList();
		}

		IEnumerable<IPublication> IPublicationFeed.Items
		{
			get { return Events; }
		}

	
	}
}
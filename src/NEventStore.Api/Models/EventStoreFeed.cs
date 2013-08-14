using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NEventStore.Api.Syndication.Atom.AtomPub;

namespace NEventStore.Api.Models
{
	public class EventStoreFeed : Resource, IPublicationFeed
	{
		public string Title { get; private set; }
		public string Summary { get; private set; }
		public string Author { get; private set; }
		public DateTime Date { get; private set; }

		public IEnumerable<Stream> Streams { get; set; }

		private EventStoreFeed()
		{
			this.Title = FeedValues.EventStoreFeed;
			this.Author = FeedValues.Author;
		}

		public static EventStoreFeed Create(string summary, DateTime date)
		{
			var feed = new EventStoreFeed();
			feed.Summary = summary;
			feed.Date = date;

			return feed;
		}

		IEnumerable<IPublication> IPublicationFeed.Items
		{
			get { return Streams; }
		}

	}
}
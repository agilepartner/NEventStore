using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NEventStore.Api.Syndication.Atom.AtomPub;

namespace NEventStore.Api.Models
{
	public class EventStoreFeed : Resource, IPublicationFeed
	{
		public string Title { get; set; }
		public string Summary { get; set; }
		public string Author { get; set; }
		public IEnumerable<Stream> Streams { get; set; }

		IEnumerable<IPublication> IPublicationFeed.Items
		{
			get { return Streams; }
		}

		public EventStoreFeed(string summary)
		{
			this.Title = FeedValues.EventStoreFeed;
			this.Summary = summary;
			this.Author = FeedValues.Author;
		}
	}
}
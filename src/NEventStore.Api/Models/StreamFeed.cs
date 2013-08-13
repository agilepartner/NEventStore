using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NEventStore.Api.Syndication.Atom.AtomPub;

namespace NEventStore.Api.Models
{
	public class StreamFeed : Resource, IPublicationFeed
	{
		public string Title { get; set; }
		public string Summary { get; set; }
		public string Author { get; set; }
		public IEnumerable<StreamInfo> Streams { get; set; }

		IEnumerable<IPublication> IPublicationFeed.Items
		{
			get { return Streams; }
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Routing;
using NEventStore.Api.Syndication.Atom;
using NEventStore.Api.Syndication.Atom.AtomPub;
using NEventStore.Api.Syndication.Atom.Links;

namespace NEventStore.Api.Models
{
	public class StreamResponseEnricher : IResponseEnricher
	{
		public bool CanEnrich(HttpResponseMessage response)
		{
			var content = response.Content as ObjectContent;

			return content != null
				&& (content.ObjectType == typeof(Stream) ||
					content.ObjectType == typeof(StreamFeed) ||
					content.ObjectType == typeof(StreamInfo) ||
					content.ObjectType == typeof(Event)
					);
		}

		public HttpResponseMessage Enrich(HttpResponseMessage response)
		{
			var url = response.RequestMessage.GetUrlHelper();

			Stream stream;
			if(response.TryGetContentValue<Stream>(out stream))
			{
				stream.Events.ForEach(e => Enrich(e, url));
				Enrich(stream, url);
				return response;
			}

			StreamFeed feed;
			if(response.TryGetContentValue<StreamFeed>(out feed))
			{
				feed.Streams.ForEach(s => Enrich(s, url));
				Enrich(feed, url);
			}

			return response;
		}

		private void Enrich(StreamFeed feed, UrlHelper url)
		{
			var selfUrl = url.Link("DefaultApi", new { controller = "stream" });
			feed.AddLink(new SelfLink(selfUrl));
		}

		private void Enrich(Stream stream, UrlHelper url)
		{
			var selfUrl = url.Link("DefaultApi", new { controller = "stream", id = stream.Id });
			stream.AddLink(new SelfLink(selfUrl));
			stream.AddLink(new EditLink(selfUrl));
		}

		private void Enrich(StreamInfo stream, UrlHelper url)
		{
			var selfUrl = url.Link("DefaultApi", new { controller = "stream", id = stream.Id });
			//var previousUrl = url.Link("DefaultApi", new { controller = "stream", id = evt.StreamId, sequence = (evt.Id == 1) ? 1 : evt.Id - 1 });
			//var nextUrl = url.Link("DefaultApi", new { controller = "stream", id = evt.StreamId, sequence = evt.Id == evt.EventCount ? evt.Id : evt.Id + 1 });
			//var firstUrl = url.Link("DefaultApi", new { controller = "stream", id = evt.StreamId, sequence = 1 });
			//var lastUrl = url.Link("DefaultApi", new { controller = "stream", id = evt.StreamId, sequence = evt.EventCount });

			stream.AddLink(new SelfLink(selfUrl));
			stream.AddLink(new EditLink(selfUrl));
			stream.AddLink(new AlternateLink(selfUrl));
			//stream.AddLink(new PreviousLink(previousUrl));
			//stream.AddLink(new NextLink(nextUrl));
			//stream.AddLink(new FirstLink(firstUrl));
			//stream.AddLink(new LastLink(lastUrl));
		}

		private void Enrich(Event evt, UrlHelper url)
		{
			var selfUrl = url.Link("DefaultApi", new { controller = "stream", id = evt.StreamId, sequence = evt.Id });
			var previousUrl = url.Link("DefaultApi", new { controller = "stream", id = evt.StreamId, sequence = (evt.Id == 1) ? 1 : evt.Id - 1 });
			var nextUrl = url.Link("DefaultApi", new { controller = "stream", id = evt.StreamId, sequence = evt.Id == evt.EventCount ? evt.Id : evt.Id + 1 });
			var firstUrl = url.Link("DefaultApi", new { controller = "stream", id = evt.StreamId, sequence = 1 });
			var lastUrl = url.Link("DefaultApi", new { controller = "stream", id = evt.StreamId, sequence = evt.EventCount });

			evt.AddLink(new SelfLink(selfUrl));
			evt.AddLink(new EditLink(selfUrl));
			evt.AddLink(new AlternateLink(selfUrl));
			evt.AddLink(new PreviousLink(previousUrl));
			evt.AddLink(new NextLink(nextUrl));
			evt.AddLink(new FirstLink(firstUrl));
			evt.AddLink(new LastLink(lastUrl));
		}

	}
}
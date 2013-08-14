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
				&& (content.ObjectType == typeof(StreamFeed) ||
					content.ObjectType == typeof(EventStoreFeed) ||
					content.ObjectType == typeof(Stream) ||
					content.ObjectType == typeof(Event)
					);
		}

		public HttpResponseMessage Enrich(HttpResponseMessage response)
		{
			var url = response.RequestMessage.GetUrlHelper();

			Event evt;
			if(response.TryGetContentValue<Event>(out evt)) 
			{
				Enrich(evt, url);
				return response;
			}

			StreamFeed stream;
			if(response.TryGetContentValue<StreamFeed>(out stream))
			{
				stream.Events.ForEach(e => Enrich(e, url));
				Enrich(stream, url);
				return response;
			}

			EventStoreFeed feed;
			if(response.TryGetContentValue<EventStoreFeed>(out feed))
			{
				feed.Streams.ForEach(s => Enrich(s, url));
				Enrich(feed, url);
			}

			return response;
		}

		private void Enrich(EventStoreFeed feed, UrlHelper url)
		{
			DateTime previous = feed.Date.AddDays(-1);
			DateTime next = feed.Date.AddDays(1);

			var selfUrl = url.Link(Routing.Routes.StreamsByDate, new { controller = Routing.Controllers.Streams, year = feed.Date.Year, month = feed.Date.Month, day = feed.Date.Day });
			var previousUrl = url.Link(Routing.Routes.StreamsByDate, new { controller = Routing.Controllers.Streams, year = previous.Year, month = previous.Month, day = previous.Day });
			var nextUrl = url.Link(Routing.Routes.StreamsByDate, new { controller = Routing.Controllers.Streams, year = next.Year, month = next.Month, day = next.Day });
			
			feed.AddLink(new SelfLink(selfUrl));
			feed.AddLink(new PreviousLink(previousUrl));
			feed.AddLink(new NextLink(nextUrl));
		}

		private void Enrich(StreamFeed stream, UrlHelper url)
		{
			var selfUrl = url.Link(Routing.Routes.Range, new { controller = Routing.Controllers.Streams, id = stream.Id, action = Routing.Actions.Range, from = stream.From, take = stream.Take });
			var previousUrl = url.Link(Routing.Routes.Range, new { controller = Routing.Controllers.Streams, id = stream.Id, action = Routing.Actions.Range, from = stream.From - stream.Take, take = stream.Take });
			var nextUrl = url.Link(Routing.Routes.Range, new { controller = Routing.Controllers.Streams, id = stream.Id, action = Routing.Actions.Range, from = stream.From + stream.Take, take = stream.Take });

			stream.AddLink(new SelfLink(selfUrl));
			stream.AddLink(new PreviousLink(previousUrl));
			stream.AddLink(new NextLink(nextUrl));
		}

		private void Enrich(Stream stream, UrlHelper url)
		{
			var selfUrl = url.Link(Routing.Routes.Range, new { controller = Routing.Controllers.Streams, id = stream.Id, action = Routing.Actions.Range, from = Routing.Head, take = Routing.PageSize });

			stream.AddLink(new SelfLink(selfUrl));
			stream.AddLink(new AlternateLink(selfUrl));
		}

		private void Enrich(Event evt, UrlHelper url)
		{
			var selfUrl = url.Link(Routing.Routes.Event, new { controller = Routing.Controllers.Streams, id = evt.StreamId, number = evt.Id });
			
			evt.AddLink(new SelfLink(selfUrl));
			evt.AddLink(new AlternateLink(selfUrl));
		}

	}
}
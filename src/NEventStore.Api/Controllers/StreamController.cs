using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using NEventStore.Api.Models;

namespace NEventStore.Api.Controllers
{
	public class StreamController : ApiController
	{
        private IStoreEvents _store;

		public StreamController(IStoreEvents store)
        {
            _store = store;
        }

		public EventStoreFeed Get() 
		{
			// TODO : set default start date
			DateTime start = DateTime.Now.AddDays(-7);

			string link = this.Url.Link("DefaultApi", null);
			Uri uri = new Uri(link);

			EventStoreFeed feed = new EventStoreFeed(String.Format("{0}@{1}", FeedValues.StreamFeedName, uri.DnsSafeHost));

			var commits = _store.Advanced.GetFrom(start).ToList();

			var streams = new List<Stream>();
			foreach(var streamId in commits.Select(c => c.StreamId).Distinct())
			{
				IEventStream stream = _store.OpenStream(streamId, 0, int.MaxValue);
				streams.Add(new Stream(stream));
			}
			feed.Streams = streams;

			return feed;
		}

		public StreamFeed Get(Guid id) 
		{
			IEventStream eventStream = _store.OpenStream(id, 0, int.MaxValue);
			StreamFeed stream = new StreamFeed(eventStream);

			return stream;
		}

		public Event Get(Guid id, int sequence)
		{
			IEventStream eventStream = _store.OpenStream(id, 0, int.MaxValue);

			if(sequence >= eventStream.CommittedEvents.Count)
				throw new HttpResponseException(HttpStatusCode.NotFound);

			EventMessage eventMessage = eventStream.CommittedEvents.ToList()[sequence - 1];
			Event evt = new Event(eventStream, eventMessage, sequence);

			return evt;
		}

	}
}
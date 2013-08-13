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

		public StreamFeed Get() 
		{
			// TODO : set default start date
			DateTime start = DateTime.Now.AddDays(-7);

			StreamFeed feed = new StreamFeed { Title = "Streams", Summary= "", Author = "EventStore" };

			var commits = _store.Advanced.GetFrom(start);

			var streams = new List<StreamInfo>();
			foreach(var streamId in commits.Select(c => c.StreamId).Distinct())
			{
				IEventStream stream = _store.OpenStream(streamId, 0, int.MaxValue);
				streams.Add(new StreamInfo(stream));
			}
			feed.Streams = streams;

			return feed;
		}

		public Stream Get(Guid id) 
		{
			IEventStream eventStream = _store.OpenStream(id, 0, int.MaxValue);
			Stream stream = new Stream(eventStream);

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
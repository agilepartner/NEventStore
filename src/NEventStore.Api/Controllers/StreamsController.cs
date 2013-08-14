using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using NEventStore.Api.Models;

namespace NEventStore.Api.Controllers
{
	public class StreamsController : ApiController
	{
		private IStoreEvents _store;

		public StreamsController(IStoreEvents store)
		{
			_store = store;
		}

		public EventStoreFeed GetEventStoreFeedHead()
		{
			DateTime date = DateTime.Now.Date;
			return GetEventStoreFeedByDate(date);
		}

		public EventStoreFeed GetEventStoreFeedByDate(int year, int month, int day)
		{
			Ensure.That<ArgumentOutOfRangeException>(year > 1900, Messages.YearOutOfRange);
			Ensure.That<ArgumentOutOfRangeException>(month >= 1 && month <= 12, Messages.MonthOutOfRange);
			var daysInMonth = DateTime.DaysInMonth(year, month);
			Ensure.That<ArgumentOutOfRangeException>(day >= 1 && day <= daysInMonth, String.Format(Messages.DayOutOfRange, daysInMonth));

			DateTime date = new DateTime(year, month, day);
			return GetEventStoreFeedByDate(date);
		}

		private EventStoreFeed GetEventStoreFeedByDate(DateTime date)
		{
			string link = this.Url.Link(Routing.Routes.Default, null);
			Uri uri = new Uri(link);

			EventStoreFeed feed = EventStoreFeed.Create(String.Format("{0}@{1}", FeedValues.StreamFeedName, uri.DnsSafeHost), date);

			var commits = _store.Advanced.GetFromTo(date, date.AddDays(1)).ToList();
			var streamData = commits.GroupBy(c => c.StreamId).Select(g => new { StreamId = g.Key, LastUpdated = g.Max(c => c.CommitStamp)  });

			var streams = new List<Stream>();
			foreach(var streamDataItem in streamData)
			{
				IEventStream stream = _store.OpenStream(streamDataItem.StreamId, 0, int.MaxValue);
				streams.Add(Stream.FromStream(stream, streamDataItem.LastUpdated));
			}
			feed.Streams = streams;

			return feed;
		}

		public StreamFeed GetStreamFeed(Guid id) 
		{
			return GetStreamFeedByRange(id, Routing.Head, Routing.PageSize);
		}

		[ActionName(Routing.Actions.Range)]
		public StreamFeed GetStreamFeedByRange(Guid id, int from, int take)
		{
			Ensure.That<ArgumentOutOfRangeException>(id != null && id != Guid.Empty, Messages.NullStreamId);
			Ensure.That<ArgumentOutOfRangeException>(from >= 0, Messages.FromOutOfRange);
			Ensure.That<ArgumentOutOfRangeException>(take >= 1, Messages.TakeOutOfRange);

			int minRevision = from;
			int maxRevision = from + take - (from > 0 ? 1 : 0);

			try
			{
				IEventStream eventStream = _store.OpenStream(id, minRevision, maxRevision);
				StreamFeed stream = StreamFeed.FromStream(eventStream, from, take);

				return stream;
			}
			catch(StreamNotFoundException)
			{
				IEventStream eventStream = _store.OpenStream(id, 0, 0);
				StreamFeed stream = StreamFeed.EmptyStream(eventStream, from, take);

				return stream;
			}
		}

		public Event GetEventByNumber(Guid id, int number)
		{
			Ensure.That<ArgumentOutOfRangeException>(id != null && id != Guid.Empty, Messages.NullStreamId);
			Ensure.That<ArgumentOutOfRangeException>(number >= 0, Messages.NumberOutOfRange);

			IEventStream eventStream = _store.OpenStream(id, number, number);

			if(eventStream.CommittedEvents.Count == 0)
				throw new HttpResponseException(HttpStatusCode.NotFound);

			EventMessage eventMessage = eventStream.CommittedEvents.First();
			Event evt = Event.FromStream(eventStream, eventMessage, number);

			return evt;
		}

	}
}
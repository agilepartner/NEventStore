using System;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Api.Syndication.Atom.AtomPub;
using NEventStore.Api.Syndication.Atom.Links;

namespace NEventStore.Api.Models
{
	public class Event : Resource, IPublication
	{
		public int Id { get; private set; }
		public Guid StreamId { get; private set; }
		public int EventCount { get; private set; }

		public string Title { get; private set; }
		public string Slug { get; private set; }
		public string Summary { get; private set; }
		public string ContentType { get; private set; }
		public object Content { get; private set; }
		public string[] Tags { get; private set; }
		public DateTime PublishDate { get; private set; }
		public DateTime LastUpdated { get; private set; }
		public string CategoriesScheme { get; private set; }

		private	 Event()
		{
			PublishDate = DateTime.UtcNow;
		}

		public static Event FromStream(IEventStream stream, EventMessage eventMessage, int id)
		{
			var @event = new Event();
			@event.Id = id;
			@event.StreamId = stream.StreamId;
			@event.EventCount = stream.CommittedEvents.Count;

			@event.Title = stream.GetEventTitle(id);
			@event.Summary = eventMessage.Body.GetType().Name;
			@event.ContentType = PublicationContentTypes.Xml;
			@event.Content = eventMessage.Body;
			@event.Tags = eventMessage.GetTags().ToArray();

			return @event;
		}

		string IPublication.Id
		{
			get { return Links.FirstOrDefault(link => link is SelfLink).Href; }
		}

		DateTime? IPublication.PublishDate
		{
			get { return PublishDate; }
		}

		IEnumerable<IPublicationCategory> IPublication.Categories
		{
			get
			{
				return Tags.Select(t => new PublicationCategory(t));
			}
		}

	}
}
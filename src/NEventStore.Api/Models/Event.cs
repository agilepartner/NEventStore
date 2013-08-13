using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NEventStore.Api.Syndication.Atom.AtomPub;
using NEventStore.Api.Syndication.Atom.Links;
using Newtonsoft.Json;

namespace NEventStore.Api.Models
{
	public class Event : Resource, IPublication
	{
		public int Id { get; set; }
		public Guid StreamId { get; set; }
		public int EventCount { get; set; }

		public string Title { get; set; }
		public string Slug { get; set; }
		public string Summary { get; set; }
		public string ContentType { get; set; }
		public string Content { get; set; }
		public string[] Tags { get; set; }
		public DateTime PublishDate { get; set; }
		public DateTime LastUpdated { get; set; }
		public string CategoriesScheme { get; set; }

		public Event()
		{
			PublishDate = DateTime.UtcNow;
		}

		public Event(IEventStream stream, EventMessage evt, int id)
			: this()
		{
			this.Id = id;
			this.StreamId = stream.StreamId;
			this.EventCount = stream.CommittedEvents.Count;

			this.Title = String.Format("{0}@{1}", id, stream.StreamId);
			this.Summary = evt.Body.GetType().Name;
			this.ContentType = "application/json";
			this.Content = JsonConvert.SerializeObject(evt.Body);
			this.Tags = evt.Headers.Select(p => String.Format("{0}={1}", p.Key, p.Value)).ToArray();
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
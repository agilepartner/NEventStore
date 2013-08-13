using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NEventStore.Api.Syndication.Atom.AtomPub;
using NEventStore.Api.Syndication.Atom.Links;
using Newtonsoft.Json;

namespace NEventStore.Api.Models
{
	public class Stream : Resource, IPublication
	{
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Slug { get; set; }
		public string Summary { get; set; }
		public string ContentType { get; set; }
		public object Content { get; set; }
		public string[] Tags { get; set; }
		public DateTime PublishDate { get; set; }
		public DateTime LastUpdated { get; set; }
		public string CategoriesScheme { get; set; }

		public Stream()
		{
			PublishDate = DateTime.UtcNow;
		}

		public Stream(IEventStream stream)
			: this()
		{
			this.Id = stream.StreamId;
			this.Title = stream.GetTitle();
			this.Summary = stream.GetSummary();
			this.ContentType = PublicationContentTypes.Text;
			this.Content = JsonConvert.SerializeObject(stream.CommittedEvents.Select(e => e.Body).ToArray());
			this.Tags = stream.GetTags().ToArray();
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
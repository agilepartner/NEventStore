using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NEventStore.Api.Syndication.Atom.AtomPub;
using NEventStore.Api.Syndication.Atom.Links;
using Newtonsoft.Json;

namespace NEventStore.Api.Models
{
	public class StreamInfo : Resource, IPublication
	{
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Summary { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public string[] Tags { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string CategoriesScheme { get; set; }

        public StreamInfo()
        {
            PublishDate = DateTime.UtcNow;
        }

		public StreamInfo(IEventStream stream)
		{
			this.Id = stream.StreamId;
			this.Title = String.Format("{0} '{1}'", stream.CommittedHeaders[Headers.AggregateTypeHeader], stream.StreamId);
			this.Summary = String.Format("StreamRevision={0};CommitSequence={1}", stream.StreamRevision, stream.CommitSequence);
			this.ContentType = "application/json";
			this.Content = JsonConvert.SerializeObject(stream.CommittedEvents.Select(e => e.Body));
			this.Tags = stream.CommittedHeaders.Select(p => String.Format("{0}={1}", p.Key, p.Value)).ToArray();
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
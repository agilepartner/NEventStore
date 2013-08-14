using System;
using System.Collections.Generic;
using System.Linq;
using NEventStore.Api.Syndication.Atom.AtomPub;
using NEventStore.Api.Syndication.Atom.Links;

namespace NEventStore.Api.Models
{
	public class Stream : Resource, IPublication
	{
		public Guid Id { get; private set; }
		public string Title { get; private set; }
		public string Slug { get; private set; }
		public string Summary { get; private set; }
		public string ContentType { get; private set; }
		public object Content { get; private set; }
		public string[] Tags { get; private set; }
		public DateTime PublishDate { get; private set; }
		public DateTime LastUpdated { get; private set; }
		public string CategoriesScheme { get; private set; }

		private Stream()
		{
			PublishDate = DateTime.UtcNow;
		}

		public static Stream FromStream(IEventStream eventStream, DateTime lastUpdated)
		{
			var stream = new Stream();

			stream.Id = eventStream.StreamId;
			stream.Title = eventStream.GetTitle();
			stream.Summary = eventStream.GetSummary();
			stream.ContentType = PublicationContentTypes.Xml;
			stream.Content = new StreamMetadata(eventStream.StreamRevision, eventStream.CommitSequence, eventStream.CommittedEvents.Count);
			stream.Tags = eventStream.GetTags().ToArray();
			stream.LastUpdated = lastUpdated;

			return stream;
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
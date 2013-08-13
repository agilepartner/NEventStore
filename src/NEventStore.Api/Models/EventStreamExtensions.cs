using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NEventStore.Api.Models
{
	internal static class EventStreamExtensions
	{
		public static string GetTitle(this IEventStream stream) 
		{
			return String.Format("{0} '{1}'", "Stream", stream.StreamId);
		}

		public static string GetEventTitle(this IEventStream stream, int id)
		{
			return String.Format("{0}@{1}", id, stream.StreamId);
		}

		public static string GetSummary(this IEventStream stream) 
		{
			if(stream.CommittedHeaders.ContainsKey(Headers.AggregateTypeHeader))
				return stream.CommittedHeaders[Headers.AggregateTypeHeader].ToString();

			return "Unknown stream type";
		}

		public static IEnumerable<string> GetTags(this IEventStream stream) 
		{
			return stream.CommittedHeaders.GetTags().Union(stream.GetStreamMetaDataTags());
		}

		public static IEnumerable<string> GetTags(this EventMessage evt) 
		{
			return evt.Headers.GetTags();
		}

		private static IEnumerable<string> GetStreamMetaDataTags(this IEventStream stream) 
		{
			yield return FormatTag("StreamRevision", stream.StreamRevision);
			yield return FormatTag("CommitSequence", stream.CommitSequence);
		}

		private static IEnumerable<string> GetTags(this IDictionary<string, object> headers)
		{
			return headers.Select(h => FormatTag(h.Key, h.Value));
		}

		private static string FormatTag(string key, object value) 
		{
			return String.Format("{0}={1}", key, value);
		}

	}
}
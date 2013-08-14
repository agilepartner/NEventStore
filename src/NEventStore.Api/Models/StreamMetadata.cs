using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NEventStore.Api.Models
{
	[Serializable]
	public class StreamMetadata
	{
		public StreamMetadata(int streamRevision, int commitSequence, int eventCount)
		{
			StreamRevision = streamRevision;
			CommitSequence = commitSequence;
			EventCount = eventCount;
		}

		public readonly int StreamRevision;
		public readonly int CommitSequence;
		public readonly int EventCount;
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NEventStore.Api
{
	public static class Routing
	{
		public const int Head = 0;
		public const int PageSize = 20;

		public static class Routes
		{
			public const string Default = "Default";
			public const string StreamsByDate = "StreamsByDate";
			public const string Range = "Range";
			public const string Event = "Event";
		}

		public static class Controllers 
		{
			public const string Streams = "streams";
		}

		public static class Actions 
		{
			public const string Range = "range";
		}
	}
}
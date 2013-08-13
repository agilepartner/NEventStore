using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace NEventStore.Api.Syndication.Atom
{
	public interface IResponseEnricher
	{
		bool CanEnrich(HttpResponseMessage response);
		HttpResponseMessage Enrich(HttpResponseMessage response);
	}
}

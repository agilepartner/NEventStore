using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.ServiceModel.Syndication;
using System.Net;
using System.IO;
using System.Xml;
using System.Net.Http.Headers;


namespace NEventStore.Api.Controllers
{
	public class ServicesController : ApiController
	{
		public HttpResponseMessage Get()
		{
			var doc = new ServiceDocument();
			var ws = new Workspace
			{
				Title = new TextSyndicationContent("EventStore"),
				BaseUri = new Uri(Request.RequestUri.GetLeftPart(UriPartial.Authority))
			};

			var streams = new ResourceCollectionInfo("Streams",
				new Uri(Url.Link("DefaultApi", new { controller = "Stream" })));

			streams.Accepts.Add("application/atom+xml;type=entry");

			// For WLW to work we need to include format in the categories URI.
			// Hoping to provide a better solution than this.
			var categoriesUri = new Uri(Url.Link("DefaultApi", new { controller = "tags", format = "atomcat" }));
			var categories = new ReferencedCategoriesDocument(categoriesUri);
			streams.Categories.Add(categories);

			ws.Collections.Add(streams);

			doc.Workspaces.Add(ws);

			var response = new HttpResponseMessage(HttpStatusCode.OK);

			var formatter = new AtomPub10ServiceDocumentFormatter(doc);

			var stream = new MemoryStream();
			using(var writer = XmlWriter.Create(stream))
			{
				formatter.WriteTo(writer);
			}

			stream.Position = 0;
			var content = new StreamContent(stream);
			response.Content = content;
			response.Content.Headers.ContentType =
				new MediaTypeHeaderValue("application/atomsvc+xml");

			return response;
		}

	}
}
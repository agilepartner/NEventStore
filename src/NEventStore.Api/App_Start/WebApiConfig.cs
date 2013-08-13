using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NEventStore.Api.Models;
using NEventStore.Api.Syndication.Atom;

namespace NEventStore.Api {
	public static class WebApiConfig {
		public static void Register(HttpConfiguration config) {
			//Handlers
			config.MessageHandlers.Add(new EnrichingHandler());
			
			//Formatters
			config.Formatters.Remove(config.Formatters.XmlFormatter);
			config.Formatters.Add(new NEventStore.Api.Syndication.Atom.AtomPub.AtomPubMediaFormatter());
			config.Formatters.JsonFormatter.SupportedMediaTypes.Clear();

			//Enricher
			config.AddResponseEnrichers(new StreamResponseEnricher());

			// Routes
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}/{sequence}",
				defaults: new
				{
					id = RouteParameter.Optional,
					sequence = RouteParameter.Optional
				}
			);

			// Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
			// To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
			// For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
			//config.EnableQuerySupport();

			// To disable tracing in your application, please comment out or remove the following line of code
			// For more information, refer to: http://www.asp.net/web-api
			config.EnableSystemDiagnosticsTracing();
		}
	}
}

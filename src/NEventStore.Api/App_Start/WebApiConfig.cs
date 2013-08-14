using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NEventStore.Api.Models;
using NEventStore.Api.Syndication.Atom;

namespace NEventStore.Api {
	public static class WebApiConfig {
		public static void Register(HttpConfiguration config) {

			config.ConfigureFilters();
			config.ConfigureHandlers();
			config.ConfigureEnrichers();
			config.ConfigureFormatters();
			config.ConfigureRoutes();

			config.EnableSystemDiagnosticsTracing();
		}

		private static void ConfigureFilters(this HttpConfiguration config)
		{
			config.Filters.Add(new NEventStore.Api.Filters.ExceptionShieldingAttribute());
		}

		private static void ConfigureHandlers(this HttpConfiguration config)
		{
			config.MessageHandlers.Add(new EnrichingHandler());
		}

		private static void ConfigureEnrichers(this HttpConfiguration config)
		{
			config.AddResponseEnrichers(new StreamResponseEnricher());
		}

		private static void ConfigureFormatters(this HttpConfiguration config)
		{
			config.Formatters.Remove(config.Formatters.XmlFormatter);
			config.Formatters.Add(new NEventStore.Api.Syndication.Atom.AtomPub.AtomPubMediaFormatter());
#if DEBUG
			config.Formatters.JsonFormatter.SupportedMediaTypes.Clear();
#endif
		}

		private static void ConfigureRoutes(this HttpConfiguration config)
		{
			config.Routes.MapHttpRoute(
				name: Routing.Routes.Default,
				routeTemplate: "{controller}/{id}",
				defaults: new
				{
					id = RouteParameter.Optional,
				}
			);

			config.Routes.MapHttpRoute(
				name: Routing.Routes.StreamsByDate,
				routeTemplate: "{controller}/{year}/{month}/{day}",
				defaults: new
				{
					controller = Routing.Controllers.Streams
				}
			);

			config.Routes.MapHttpRoute(
				name: Routing.Routes.Event,
				routeTemplate: "{controller}/{id}/{number}",
				defaults: new
				{
					controller = Routing.Controllers.Streams
				}
			);

			config.Routes.MapHttpRoute(
				name: Routing.Routes.Range,
				routeTemplate: "{controller}/{id}/{action}/{from}/{take}",
				defaults: new
				{
					controller = Routing.Controllers.Streams,
				}
			);
		}

	}
}

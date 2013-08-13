using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.WebApi;
using NEventStore.Persistence.SqlPersistence.SqlDialects;

namespace NEventStore.Api {
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class WebApiApplication : System.Web.HttpApplication {
		protected void Application_Start() {

			var builder = new ContainerBuilder();

			var wireup = StoreWireup();
			builder.Register(c => wireup.Build()).InstancePerApiRequest();

			builder.RegisterApiControllers(typeof(MvcApplication).Assembly);

			var container = builder.Build();

			var resolver = new AutofacWebApiDependencyResolver(container);
			GlobalConfiguration.Configuration.DependencyResolver = resolver;
			
			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
		}

		private Wireup StoreWireup()
		{
			return Wireup.Init()
			   .UsingSqlPersistence("EventStore")
				   .WithDialect(new MsSqlDialect())
				   .UsingJsonSerialization()
					   .Compress()
					   .EncryptWith(Encryption.Key);
		}
	}
}
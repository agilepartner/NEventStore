using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
			builder.Register(c => wireup.Build()).SingleInstance();

			builder.RegisterApiControllers(typeof(Routing).Assembly);

			var container = builder.Build();

			var resolver = new AutofacWebApiDependencyResolver(container);
			GlobalConfiguration.Configuration.DependencyResolver = resolver;
			
			AreaRegistration.RegisterAllAreas();

			WebApiConfig.Register(GlobalConfiguration.Configuration);
		}

		private Wireup StoreWireup()
		{
			return Wireup.Init()
				.LogToOutputWindow()
				.UsingSqlPersistence("EventStore")
				   .WithDialect(new MsSqlDialect())
				.UsingJsonSerialization(() => LoadEventLibraryTypes())
					.Compress()
					.EncryptWith(Encryption.Key);
		}

		private IEnumerable<Type> LoadEventLibraryTypes()
		{
			//System.Diagnostics.Debugger.Break();

			List<Type> types = new List<Type>();
			//var pluginDir = HttpContext.Current.Server.MapPath("~/Events");
			//foreach(var assemblyFile in Directory.GetFiles(pluginDir, "*.dll"))
			//{
			//	var assembly = Assembly.LoadFile(assemblyFile);
			//	foreach(var type in assembly.GetTypes())
			//	{
			//		types.Add(type);
			//		//if(!type.IsSubclassOf(typeof(IEnumerable)))
			//		//{
			//		//	var enumerableType = typeof(IEnumerable<>).MakeGenericType(type);
			//		//	types.Add(enumerableType);
			//		//}

			//	}
			//}
			return types;
		}
	}
}
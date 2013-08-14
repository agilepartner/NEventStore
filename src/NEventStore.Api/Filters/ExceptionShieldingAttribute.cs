using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using NEventStore.Logging;

namespace NEventStore.Api.Filters
{
	public class ExceptionShieldingAttribute : System.Web.Http.Filters.ExceptionFilterAttribute
	{
		private static readonly ILog Logger = LogFactory.BuildLogger(typeof(ExceptionShieldingAttribute));

		public override void OnException(System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext)
		{
			Logger.Error(actionExecutedContext.Exception.ToString());

			HandleArgumentException(actionExecutedContext);
			HandleGeneralException(actionExecutedContext);
		}

		private static void HandleGeneralException(System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext)
		{
			if(actionExecutedContext.Exception is Exception)
			{
				throw new HttpResponseException(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
				{
					Content = new StringContent(actionExecutedContext.Exception.ToString()),
					ReasonPhrase = Messages.GeneralExceptionMessage
				});
			}
		}

		private static void HandleArgumentException(System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext)
		{
			var argumentException = actionExecutedContext.Exception as ArgumentException;
			if(argumentException != null)
			{
				throw new HttpResponseException(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
				{
					Content = new StringContent(argumentException.ToString()),
					ReasonPhrase = argumentException.Message
				});
			}
		}
	}
}
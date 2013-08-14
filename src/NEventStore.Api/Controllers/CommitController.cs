using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;
using NEventStore.Persistence;

namespace NEventStore.Api.Controllers
{
	public class CommitController : ApiController
	{
		private IStoreEvents _store;

		public CommitController(IStoreEvents store)
		{
			_store = store;
		}

		public HttpResponseMessage Post([FromBody]Commit commit)
		{
			if(commit == null)
				return new HttpResponseMessage(HttpStatusCode.BadRequest);

			using(TransactionScope scope = new TransactionScope())
			{
				try
				{
					_store.Advanced.Commit(commit);
					_store.Advanced.MarkCommitAsDispatched(commit);
					scope.Complete();

					return CreatedResponse(commit);
				}
				catch(DuplicateCommitException)
				{
					return CreatedResponse(commit);
				}
				catch(ConcurrencyException)
				{
					throw new HttpResponseException(HttpStatusCode.Conflict);
				}
				catch(StorageException)
				{
					throw new HttpResponseException(HttpStatusCode.InternalServerError);
				}
			}
		}

		private HttpResponseMessage CreatedResponse(Commit commit)
		{
			var response = this.Request.CreateResponse(HttpStatusCode.Created);
			string uri = Url.Link(Routing.Routes.Range, new
			{
				controller = Routing.Controllers.Streams,
				id = commit.StreamId,
				action = Routing.Actions.Range,
				from = commit.StreamRevision - commit.Events.Count,
				take = commit.Events.Count
			});
			response.Headers.Location = new Uri(uri);

			return response;
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web;
using System.Web.Http;
using NEventStore.Persistence;

namespace NEventStore.Api.Controllers
{
	public class DispatchController : ApiController
	{
		private IStoreEvents _store;

		public DispatchController(IStoreEvents store)
		{
			_store = store;
		}

		public HttpResponseMessage Post([FromBody]Commit commit)
		{
			if(commit == null)
				return new HttpResponseMessage(HttpStatusCode.BadRequest);

			using(TransactionScope scope = new TransactionScope())
			{
				IEventStream stream = PrepareStream(commit);

				try
				{
					stream.CommitChanges(commit.CommitId);
					scope.Complete();

					return CreatedResponse(commit);
				}
				catch(DuplicateCommitException)
				{
					//Idempotency
					stream.ClearChanges();
					return CreatedResponse(commit);
				}
				catch(ConcurrencyException)
				{
					stream.ClearChanges();
					throw new HttpResponseException(HttpStatusCode.Conflict);
				}
				catch(StorageException)
				{
					stream.ClearChanges();
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

		private IEventStream PrepareStream(Commit commit)
		{
			IEventStream stream = _store.OpenStream(commit.StreamId, 0, int.MaxValue);

			foreach(var item in commit.Headers)
				stream.UncommittedHeaders[item.Key] = item.Value;

			foreach(var evt in commit.Events)
				stream.Add(evt);

			return stream;
		}
	}
}
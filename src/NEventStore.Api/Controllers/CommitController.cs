using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Transactions;
using System.Web.Http;

namespace NEventStore.Api.Controllers
{
	public class CommitController : ApiController
	{
		private IStoreEvents _store;

		public CommitController(IStoreEvents store)
		{
			_store = store;
		}

		public void Post([FromBody]Commit commit)
		{
			if(commit == null)
				return;

			using(TransactionScope scope = new TransactionScope())
			{
				_store.Advanced.Commit(commit);
				_store.Advanced.MarkCommitAsDispatched(commit);
				scope.Complete();
			}
		}

	}
}
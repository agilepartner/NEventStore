//
// SinglePhaseEnlistment.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//	Ankit Jain	 <JAnkit@novell.com>
//
// (C)2005 Novell Inc,
// (C)2006 Novell Inc,
//


namespace System.Transactions
{
	public class SinglePhaseEnlistment : Enlistment
	{
//		bool committed;
		Transaction tx;
		ISinglePhaseNotification enlisted;
		
		internal SinglePhaseEnlistment (Transaction tx, ISinglePhaseNotification enlisted)
		{
			this.tx = tx;
			this.enlisted = enlisted;
		}

		public void Aborted ()
		{
			Aborted (null);
		}

		public void Aborted (Exception e)
		{
			tx.Rollback (e, enlisted);
		}

		public void Committed ()
		{
			/* FIXME */
//			committed = true;
		}

		public void InDoubt ()
		{
			throw new NotImplementedException ();
		}

		public void InDoubt (Exception e)
		{
			throw new NotImplementedException ();
		}
	}
}


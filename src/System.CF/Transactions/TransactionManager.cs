//
// TransactionManager.cs
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
	public static class TransactionManager
	{
		/* 60 secs */
		static TimeSpan defaultTimeout = new TimeSpan (0, 1, 0);
		/* 10 mins */
		static TimeSpan maxTimeout = new TimeSpan (0, 10, 0);

		public static TimeSpan DefaultTimeout {
			get { return defaultTimeout; }
		}

		public static HostCurrentTransactionCallback HostCurrentCallback {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public static TimeSpan MaximumTimeout {
			get { return maxTimeout; }
		}

		public static void RecoveryComplete (Guid manager)
		{
			throw new NotImplementedException ();
		}

		public static Enlistment Reenlist (Guid manager,
			byte[] recoveryInfo,
			IEnlistmentNotification notification)
		{
			throw new NotImplementedException ();
		}

		public static event TransactionStartedEventHandler
			DistributedTransactionStarted;
	}
}


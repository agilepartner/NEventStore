//
// TransactionEventArgs.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//


namespace System.Transactions
{
	public class TransactionEventArgs : EventArgs
	{
		public Transaction Transaction {
			get { throw new NotImplementedException (); }
		}
	}
}


//
// TransactionInterop.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//


// OK, everyone knows that am not interested in DTC support ;-)

namespace System.Transactions
{
	public static class TransactionInterop
	{
		public static IDtcTransaction GetDtcTransaction (Transaction transaction)
		{
			throw new NotImplementedException ();
		}

		public static byte [] GetExportCookie (Transaction transaction,
			byte [] exportCookie)
		{
			throw new NotImplementedException ();
		}

		public static Transaction GetTransactionFromDtcTransaction (
			IDtcTransaction dtc)
		{
			throw new NotImplementedException ();
		}

		public static Transaction GetTransactionFromExportCookie (
			byte [] exportCookie)
		{
			throw new NotImplementedException ();
		}

		public static Transaction GetTransactionFromTransmitterPropagationToken (byte [] token)
		{
			throw new NotImplementedException ();
		}

		public static byte [] GetTransmitterPropagationToken (
			Transaction transaction)
		{
			throw new NotImplementedException ();
		}

		public static byte [] GetWhereabouts ()
		{
			throw new NotImplementedException ();
		}
	}
}


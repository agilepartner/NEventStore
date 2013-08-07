//
// DependentTransaction.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

using System.Runtime.Serialization;

namespace System.Transactions
{
	[Serializable]
	public sealed class DependentTransaction : Transaction {
//		Transaction parent;
//		DependentCloneOption option;
		bool completed;

		internal DependentTransaction (Transaction parent,
			DependentCloneOption option)
		{
//			this.parent = parent;
//			this.option = option;
		}

		internal bool Completed {
			get { return completed; }
		}

		public void Complete ()
		{
			throw new NotImplementedException ();
		}

	}
}


using System;
using System.Collections.Generic;
using QSOrmProject;

namespace QSBanks
{
	public abstract class AccountOwnerBase : PropertyChangedBase, IAccountOwner
	{
		IList<Account> accounts;

		public virtual IList<Account> Accounts {
			get { return accounts; }
			set {
				accounts = value;
				UpdateDefault ();
			}
		}

		Account defaultAccount;

		public virtual Account DefaultAccount {
			get { return defaultAccount; }
			set {
				defaultAccount = value;
				UpdateDefault ();
			}
		}

		public AccountOwnerBase ()
		{
		}

		private void UpdateDefault ()
		{
			if (Accounts == null)
				return;
			foreach (Account acc in Accounts) {
				acc.IsDefault = (acc == DefaultAccount);
			}
		}
	}

	public interface IAccountOwner
	{
		Account DefaultAccount { get; set; }

		IList<Account> Accounts { get; set; }
	}
}


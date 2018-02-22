using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using QSOrmProject;
using System.Linq;

namespace QSBanks
{
	public abstract class AccountOwnerBase : PropertyChangedBase, IAccountOwner
	{
		IList<Account> accounts;

		public virtual IList<Account> Accounts {
			get { return accounts; }
			set {
				if(SetField(ref accounts, value, () => Accounts)) {
					FillOwnerForAccounts();
					UpdateDefault();
					observableAccounts = null;
				}
			}
		}

		GenericObservableList<Account> observableAccounts;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Account> ObservableAccounts {
			get {
				if(observableAccounts == null) {
					observableAccounts = new GenericObservableList<Account>(Accounts);
					UpdateDefault();
				}
				return observableAccounts;
			}
		}

		Account defaultAccount;

		[Display(Name = "Основной счет")]
		public virtual Account DefaultAccount {
			get { return defaultAccount; }
			set {
				if(SetField(ref defaultAccount, value, () => DefaultAccount)) {
					UpdateDefault();
				}
			}
		}

		public AccountOwnerBase()
		{
			accounts = new List<Account>();
		}

		public virtual void AddAccount(Account account)
		{
			ObservableAccounts.Add(account);
			account.Owner = this;
			if(DefaultAccount == null)
				DefaultAccount = account;
		}

		#region Внутренние методы

		private void UpdateDefault()
		{
			if(Accounts == null || !NHibernateUtil.IsInitialized(Accounts))
				return;
			foreach(Account acc in Accounts) {
				acc.IsDefault = (DomainHelper.EqualDomainObjects(acc, DefaultAccount));
			}
			if(DefaultAccount == null && Accounts.Count > 0)
				DefaultAccount = Accounts.Where(x => !x.Inactive).FirstOrDefault();
			if(DefaultAccount == null && Accounts.Count > 0)
				DefaultAccount = Accounts.First();
		}

		private void FillOwnerForAccounts()
		{
			foreach(var acc in Accounts)
                acc.Owner = this;
		}

  		#endregion
	}

	public interface IAccountOwner
	{
		Account DefaultAccount { get; set; }

		IList<Account> Accounts { get;}

		GenericObservableList<Account> ObservableAccounts { get;}
	}
}


using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace QS.Banks.Domain
{
	public abstract class AccountOwnerBase : PropertyChangedBase, IAccountOwner
	{
		IObservableList<Account> accounts = new ObservableList<Account>();
		private Account defaultAccount;

		public virtual IObservableList<Account> Accounts {
			get { return accounts; }
			set {
				SetField(ref accounts, value, () => Accounts);
			}
		}
		public virtual IObservableList<Account> ObservableAccounts => Accounts;

		[Display(Name = "Основной счет")]
		public virtual Account DefaultAccount {
			get => defaultAccount;
			set => SetField(ref defaultAccount, value);
		}

		public virtual void AddAccount(Account account)
		{
			Accounts.Add(account);
			account.Owner = this;
			if(DefaultAccount == null)
				account.IsDefault = true;
		}

		#region Внутренние методы


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

		IObservableList<Account> Accounts { get;}

		void AddAccount(Account account);
	}
}


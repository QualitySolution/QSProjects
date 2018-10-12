using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;

namespace QSBanks
{
	public abstract class AccountOwnerBase : PropertyChangedBase, IAccountOwner
	{
		IList<Account> accounts;

		public virtual IList<Account> Accounts {
			get { return accounts; }
			set {
				SetField(ref accounts, value, () => Accounts);
			}
		}

		GenericObservableList<Account> observableAccounts;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Account> ObservableAccounts {
			get {
				if(observableAccounts == null) {
					observableAccounts = new GenericObservableList<Account>(Accounts);
				}
				return observableAccounts;
			}
		}

		[Display(Name = "Основной счет")]
		public virtual Account DefaultAccount {
			get{
				return ObservableAccounts.FirstOrDefault(x => x.IsDefault);
			}
			set{
				Account oldDefAccount = ObservableAccounts.FirstOrDefault(x => x.IsDefault);
				if(oldDefAccount != null && value != null && oldDefAccount.Id != value.Id) {
					oldDefAccount.IsDefault = false;
				}
				value.IsDefault = true;
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

		IList<Account> Accounts { get;}

		GenericObservableList<Account> ObservableAccounts { get;}
	}
}


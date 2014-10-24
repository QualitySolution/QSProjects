using System;
using QSOrmProject;
using System.Data.Bindings;

namespace QSBanks
{
	[OrmSubjectAttibutes("Банковский счет")]
	public class Account : BaseNotifyPropertyChanged
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Number { get; set; }

		Bank inBank;
		public virtual Bank InBank {
			get {
				return inBank;
			}
			set {
				if (inBank == value)
					return;
				inBank = value;
				OnPropertyChanged ("InBank");
			}
		}
		#endregion

		public bool IsDefault { get; set; }

		public virtual string BankName{
			get{
				if (InBank == null)
					return String.Empty;
				else
					return InBank.Name;
			}
		}

		public Account()
		{
			Name = String.Empty;
			Number = String.Empty;
		}

		public override bool Equals(Object obj)
		{
			Account accountObj = obj as Account; 
			if (accountObj == null)
				return false;
			else
				return Id.Equals(accountObj.Id);
		}

		public override int GetHashCode()
		{
			return this.Id.GetHashCode(); 
		}
	}
}


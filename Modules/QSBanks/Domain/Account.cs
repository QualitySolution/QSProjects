using System;
using System.Data.Bindings;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.Text.RegularExpressions;

namespace QSBanks
{
	[OrmSubject ("Счета")]
	public class Account : BaseNotifyPropertyChanged, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		public virtual string Name { get; set; }

		[StringLength (25, MinimumLength = 20, ErrorMessage = "Номер банковского счета должен содержать 20 цифр и не превышать 25-ти.")]
		public virtual string Number { get; set; }

		public virtual bool Inactive { get; set; }

		Bank inBank;

		[Required (ErrorMessage = "Банк должен быть заполнен.")]
		public virtual Bank InBank {
			get { return inBank; }
			set {
				if (inBank == value)
					return;
				inBank = value;
				OnPropertyChanged ("InBank");
			}
		}

		#endregion

		public virtual bool IsDefault { get; set; }

		public virtual string BankName {
			get{
				if (InBank == null)
					return String.Empty;
				else
					return InBank.Name;
			}
		}

		public Account ()
		{
			Name = String.Empty;
			Number = String.Empty;
		}

		public override bool Equals (Object obj)
		{
			Account accountObj = obj as Account; 
			if (accountObj == null)
				return false;
			else
				return Id.Equals (accountObj.Id);
		}

		public override int GetHashCode ()
		{
			return this.Id.GetHashCode (); 
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (!new Regex (@"^[0-9]*$").IsMatch (Number))
				yield return new ValidationResult ("Номер счета может содержать только цифры.", new[]{ "Number" });
		}

		#endregion
	}
}


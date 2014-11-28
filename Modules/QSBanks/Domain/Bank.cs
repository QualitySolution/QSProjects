using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.Text.RegularExpressions;

namespace QSBanks
{
	[OrmSubjectAttributes("Банки")]
	public class Bank: IValidatableObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		[Required(ErrorMessage = "Название банка должно быть заполнено.")]
		public virtual string Name { get; set; }
		[StringLength(9, MinimumLength = 9, ErrorMessage = "Бик должен состоять из 8 цифр.")]
		public virtual string Bik { get; set; }
		[StringLength(25, MinimumLength = 20, ErrorMessage = "Номер кореспондентского счета должен содержать 20 цифр и не превышать 25-ти.")]
		public virtual string CorAccount { get; set; }
		[Required(ErrorMessage = "Город является обязательным полем.")]
		public virtual string City { get; set; }
		[Required(ErrorMessage = "Регион является обязательным полем.")]
		public virtual string Region { get; set; }
		#endregion

		public Bank()
		{
			Name = String.Empty;
			Bik = String.Empty;
			CorAccount = String.Empty;
			City = String.Empty;
			Region = String.Empty;
		}

		public override bool Equals(Object obj)
		{
			Bank accountObj = obj as Bank; 
			if (accountObj == null)
				return false;
			else
				return Id.Equals(accountObj.Id);
		}

		public override int GetHashCode()
		{
			return this.Id.GetHashCode(); 
		}

		#region IValidatableObject implementation
		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (!new Regex (@"^[0-9]*$").IsMatch (Bik))
				yield return new ValidationResult ("Бик должен содержать только цифры.", new[]{"Bik"});
			if (!new Regex(@"^[0-9]*$").IsMatch (CorAccount))
				yield return new ValidationResult ("Корреспондентский счет должен содержать только цифры.", new[]{"CorAccount"});
		}
		#endregion
	}
}


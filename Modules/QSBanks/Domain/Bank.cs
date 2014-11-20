using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace QSBanks
{
	[OrmSubjectAttibutes("Банки")]
	public class Bank
	{
		#region Свойства
		public virtual int Id { get; set; }
		[Required(ErrorMessage = "Название банка должно быть заполнено.")]
		public virtual string Name { get; set; }
		[StringLength(9, MinimumLength = 9, ErrorMessage = "Бик должен состоять из 8 цифр.")]
		[Digits(ErrorMessage = "Бик должен содержать только цифры.")]
		public virtual string Bik { get; set; }
		[StringLength(25, MinimumLength = 20, ErrorMessage = "Номер кореспондентского счета должен содержать 20 цифр и не превышать 25-ти.")]
		[Digits(ErrorMessage = "Кореспондентский счет может содержать только цифры.")]
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

	}
}


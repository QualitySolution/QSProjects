using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace QS.BusinessCommon.Domain
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "единицы измерения",
		Nominative = "единица измерения")]
	[EntityPermission]
	public class MeasurementUnits : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название должно быть заполнено.")]
		[StringLength (10)]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		short digits;

		[Display (Name = "Знаков после запятой")]
		public virtual short Digits {
			get { return digits; }
			set { SetField (ref digits, value, () => Digits); }
		}

		string okei;
			
		[StringLength (5)]
		[Display (Name = "Код ОКЕИ")]
		public virtual string OKEI {
			get { return okei; }
			set { SetField (ref okei, value, () => OKEI); }
		}

		#endregion

		#region additions

		public virtual string MakeAmountShortStr(int amount)
		{
			return String.Format ("{0} {1}", amount, Name);
		}

		public virtual string MakeAmountShortStr(decimal amount)
		{
			return String.Format ("{0:" + String.Format ("F{0}", Digits) + "} {1}", 
				amount,
				Name);
		}

		#endregion

		public MeasurementUnits ()
		{
			Name = String.Empty;
		}
	}
}


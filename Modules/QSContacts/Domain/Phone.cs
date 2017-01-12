using System;
using QSOrmProject;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace QSContacts
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "телефоны",
		Nominative = "телефон")]
	public class Phone : PropertyChangedBase
	{
		#region Свойства
		public virtual int Id { get; set; }
		string number;
		public virtual string Number
		{
			get
			{
				return number;
			}
			set
			{
				if(SetField (ref number, value, () => Number))
					DigitsNumber = Regex.Replace (Number, "[^0-9]", "");
			}
		}

		private string digitsNumber;

		[Display (Name = "Только цифры")]
		public virtual string DigitsNumber {
		    get { return digitsNumber; }
			protected set { SetField (ref digitsNumber, value, () => DigitsNumber); }
		}

		public virtual string Additional { get; set; }

		private PhoneType numberType;
		public virtual PhoneType NumberType
		{
			get
			{
				return numberType;
			}
			set
			{
				SetField(ref numberType, value, () => NumberType);
			}
		}
		#endregion

		public Phone()
		{
			if (String.IsNullOrWhiteSpace(QSContactsMain.DefaultCityCode))
				Number = String.Empty;
			else
				Number = String.Format("({0})", QSContactsMain.DefaultCityCode);
			Additional = String.Empty;
		}

		public override string ToString()
		{
			return Number;
		}
	}
}


using System;
using QSOrmProject;
using System.Text.RegularExpressions;

namespace QSContacts
{
	[OrmSubject("Телефоны")]
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
		public virtual string DigitsNumber { get; private set; }
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


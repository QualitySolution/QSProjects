using System;
using QSOrmProject;
using System.Data.Bindings;
using System.Text.RegularExpressions;

namespace QSContacts
{
	[OrmSubjectAttributes("Телефоны")]
	public class Phone : BaseNotifyPropertyChanged
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
				number = value;
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
				if (numberType == value)
					return;
				numberType = value;
				OnPropertyChanged("NumberType");
			}
		}
		#endregion

		public Phone()
		{
			Number = String.Empty;
			Additional = String.Empty;
		}
	}
}


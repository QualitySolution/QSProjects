using System;
using QSOrmProject;
using System.Data.Bindings;

namespace QSPhones
{
	[OrmSubjectAttibutes("Телефоны")]
	public class Phone : BaseNotifyPropertyChanged
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Number { get; set; }
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


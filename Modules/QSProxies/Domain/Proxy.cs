using System;
using QSOrmProject;
using System.Data.Bindings;

namespace QSProxies
{
	[OrmSubjectAttibutes("Доверенности")]
	public class Proxy : BaseNotifyPropertyChanged, IDomainObject
	{
		#region Свойства
		public virtual int Id { get; set; }
		public virtual string Number { get; set; }
		public virtual DateTime IssueDate { get; set; }
		public virtual DateTime StartDate { get; set; }
		public virtual DateTime ExpirationDate { get; set; }
		#endregion

		public Proxy ()
		{
			Number = String.Empty;
		}
	}
}


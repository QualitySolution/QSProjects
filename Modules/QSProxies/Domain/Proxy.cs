using System;
using QSOrmProject;
using System.Data.Bindings;
using System.Collections.Generic;
using QSContacts;

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
		public virtual IList<Person> Persons { get; set; }
		#endregion

		public Proxy ()
		{
			Number = String.Empty;
		}
		public string Issue { get { return IssueDate.ToShortDateString(); } } 
		public string Start { get { return StartDate.ToShortDateString(); } } 
		public string Expiration { get { return ExpirationDate.ToShortDateString(); } } 
	}
}


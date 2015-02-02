using System;
using NHibernate;

namespace QSOrmProject
{
	public interface IReferenceFilter
	{
		ISession Session { set; get;}
		ICriteria BaseCriteria { set; get;}
		ICriteria FiltredCriteria { get;}

		event EventHandler Refiltered;

		bool IsFiltred { get;}
	}
}


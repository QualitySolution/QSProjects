using System;
using NHibernate;
using QS.DomainModel.UoW;

namespace QSOrmProject
{
	public interface IReferenceFilter
	{
		IUnitOfWork UoW { set; get;}
		ICriteria BaseCriteria { set; get;}
		ICriteria FiltredCriteria { get;}

		event EventHandler Refiltered;

		bool IsFiltred { get;}
	}
}


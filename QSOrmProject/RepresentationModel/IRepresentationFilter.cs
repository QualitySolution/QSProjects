using System;
using NHibernate;

namespace QSOrmProject.RepresentationModel
{
	public interface IRepresentationFilter
	{
		IUnitOfWork UoW { get;}

		event EventHandler Refiltered;
	}
}


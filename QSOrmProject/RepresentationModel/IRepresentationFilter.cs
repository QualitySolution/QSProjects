using System;
using QS.DomainModel.UoW;

namespace QSOrmProject.RepresentationModel
{
	public interface IRepresentationFilter
	{
		IUnitOfWork UoW { get;}

		event EventHandler Refiltered;
	}
}


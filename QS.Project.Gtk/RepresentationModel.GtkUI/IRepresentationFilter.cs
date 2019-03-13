using System;
using QS.DomainModel.UoW;

namespace QS.RepresentationModel.GtkUI
{
	public interface IRepresentationFilter
	{
		IUnitOfWork UoW { get;}

		event EventHandler Refiltered;
	}
}


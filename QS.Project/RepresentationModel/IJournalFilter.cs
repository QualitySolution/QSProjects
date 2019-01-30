using System;
using QS.DomainModel.UoW;

namespace QS.RepresentationModel
{
	public interface IJournalFilter
	{
		IUnitOfWork UoW { get;}

		event EventHandler Refiltered;
	}
}


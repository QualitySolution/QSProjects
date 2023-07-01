﻿using NHibernate;
using QS.DomainModel.UoW;

namespace QS.DomainModel.Tracking
{
	public interface IUnitOfWorkTracked
    {
        ISession Session { get; }
        UnitOfWorkTitle ActionTitle { get; }

		SingleUowEventsTracker EventsTracker { get; }
	}
}

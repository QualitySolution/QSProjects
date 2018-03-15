using System;
using NHibernate;
using NHibernate.Event;

namespace QS.DomainModel
{
	internal interface IUnitOfWorkEventHandler
	{
		ISession Session { get; }

		void OnPostLoad(PostLoadEvent loadEvent);
		void OnPreLoad(PreLoadEvent loadEvent);
		void OnPostDelete(PostDeleteEvent deleteEvent);
	}
}

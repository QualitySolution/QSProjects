using System;
using NHibernate;

namespace QSOrmProject
{
	[Obsolete("Будет удалено после окончательного переезда")]
	public interface IOrmDialog
	{
		[Obsolete("Будет удалено после окончательного переезда")]
		ISession Session { get; }

		object Subject { get; }
	}

	public interface IOrmDialogNew
	{
		[Obsolete("Только для совместитомсти. Будет удалено после окончательного переезда.")]
		ISession Session { get; }

		IUnitOfWork UoW { get; }
		object Subject { get; }
	}

	public interface IOrmSlaveDialog
	{
		OrmParentReference ParentReference { get; set;}
		object Subject { get; set; }
	}
}


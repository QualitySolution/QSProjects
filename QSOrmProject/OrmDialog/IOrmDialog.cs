using System;
using NHibernate;

namespace QSOrmProject
{
	[Obsolete ("Будет удалено после окончательного переезда")]
	public interface IOrmDialog
	{
		[Obsolete ("Будет удалено после окончательного переезда")]
		ISession Session { get; }

		object EntityObject { get; }
	}

	public interface IOrmDialogNew
	{
		IUnitOfWork UoW { get; }

		object EntityObject { get; }
	}

	public interface IOrmSlaveDialog
	{
		OrmParentReference ParentReference { get; set; }

		object Subject { get; set; }
	}

	public interface IEditableDialog
	{
		bool IsEditable { get; set; }
	}
}


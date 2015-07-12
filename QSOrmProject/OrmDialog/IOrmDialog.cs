using System;
using NHibernate;

namespace QSOrmProject
{
	public interface IOrmDialog
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


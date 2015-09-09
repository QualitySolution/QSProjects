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
		object Subject { get; set; }
	}

	public interface IEditableDialog
	{
		bool IsEditable { get; set; }
	}
}


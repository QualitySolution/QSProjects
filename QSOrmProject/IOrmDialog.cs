using System;
using NHibernate;

namespace QSOrmProject
{
	public interface IOrmDialog
	{
		ISession Session { get; }
		object Subject { get; set; }
	}

	public interface IOrmSlaveDialog
	{
		OrmParentReference ParentReference { get; set;}
		object Subject { get; set; }
	}
}


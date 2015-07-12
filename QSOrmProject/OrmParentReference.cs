using System;
using NHibernate;
using System.Collections;

namespace QSOrmProject
{
	public class OrmParentReference
	{
		public IUnitOfWork UoW;
		public object ParentObject{ get; private set;}
		public string ListProperty{ get; private set;}

		public OrmParentReference (IUnitOfWork uow, object parent, string listProperty)
		{
			UoW = uow;
			var objectType = NHibernateUtil.GetClass (parent);
			if(objectType.GetProperty (listProperty) == null)
				throw new ArgumentException (String.Format ("Класс {0}, не содержит свойства {1}.", objectType, listProperty));
			this.ParentObject = parent;
			this.ListProperty = listProperty;
		}

		public IList List {
			get{
				var objectType = NHibernateUtil.GetClass (ParentObject);
				return (IList) objectType.GetProperty (ListProperty).GetValue (ParentObject, null);
			}
		}
	}
}


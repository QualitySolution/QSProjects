using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QS.DomainModel.Entity
{
	public abstract class DomainTreeNodeBase<TEntity> : PropertyChangedBase, IDomainObject 
		where TEntity : DomainTreeNodeBase<TEntity>
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Свойства

		public virtual int Id { get; set; }

		TEntity parent;

		[Display(Name = "Родитель")]
		public virtual TEntity Parent {
			get { return parent; }
			set {
				if(NHibernate.NHibernateUtil.IsInitialized(Parent)) {
					if(parent != null)
						parent.Childs.Remove((TEntity)this);
					if(value != null && value.CheckCircle((TEntity)this)) {
						logger.Warn("Родитель не назначен, так как возникает зацикливание дерева.");
						return;
					}
				}

				SetField(ref parent, value, () => Parent);

				if(parent != null && NHibernate.NHibernateUtil.IsInitialized(Parent))
					parent.Childs.Add((TEntity)this);
			}
		}

		IList<TEntity> childs = new List<TEntity>();

		[Display(Name = "Дочерние элементы")]
		public virtual IList<TEntity> Childs {
			get { return childs; }
			set { SetField(ref childs, value, () => Childs); }
		}

		#endregion

		#region Внутренние

		private bool CheckCircle(TEntity node)
		{
			if(Parent == null)
				return false;

			if(Parent == node)
				return true;

			return Parent.CheckCircle(node);
		}

		#endregion
	}
}

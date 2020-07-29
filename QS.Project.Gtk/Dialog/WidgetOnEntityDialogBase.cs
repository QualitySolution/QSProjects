using System;
using QS.DomainModel.Entity;

namespace QS.Dialog.Gtk
{
	public class WidgetOnEntityDialogBase<TEntity> : WidgetOnDialogBase
		where TEntity : IDomainObject, new()
	{
		public TEntity RootEntity {
			get {
				return EntityDialog.Entity;
			}
		}

		public EntityDialogBase<TEntity> EntityDialog{ get {
				return MyEntityDialog as EntityDialogBase<TEntity>;
			}
		}
	}
}

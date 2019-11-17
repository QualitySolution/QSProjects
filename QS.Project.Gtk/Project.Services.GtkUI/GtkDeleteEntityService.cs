using System;
using QS.Deletion;

namespace QS.Project.Services.GtkUI
{
	public class GtkDeleteEntityService : IDeleteEntityService
	{
		//FIXME Сейчас это просто прослойка к старому механизму удаления. Надо будет его переделать отеделив от Gtk.
		public GtkDeleteEntityService()
		{
		}

		public bool DeleteEntity<TEntity>(int id)
		{
			return DeleteHelper.DeleteEntity(typeof(TEntity), id);
		}

		public bool DeleteEntity(Type clazz, int id)
		{
			return DeleteHelper.DeleteEntity(clazz, id);
		}
	}
}

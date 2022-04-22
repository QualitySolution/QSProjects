using System;

namespace QSOrmProject.UpdateNotification
{
	[Obsolete("Используйте новый механизм уведомлений об изменениях сущностей QS.DomainModel.NotifyChange.")]
	public class OrmObjectUpdatedGenericEventArgs<TEntity> : EventArgs
	{
		public TEntity[] UpdatedSubjects { get; private set; }

		public OrmObjectUpdatedGenericEventArgs (params TEntity[] updatedSubjects)
		{
			UpdatedSubjects = updatedSubjects;
		}
	}
}


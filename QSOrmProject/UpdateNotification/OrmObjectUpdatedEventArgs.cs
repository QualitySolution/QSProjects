using System;

namespace QSOrmProject.UpdateNotification
{
	[Obsolete("Используйте новый механизм уведомлений об изменениях сущьностей QS.DomainModel.NotifyChange.")]
	public class OrmObjectUpdatedEventArgs : EventArgs
	{
		public object[] UpdatedSubjects { get; private set; }

		public OrmObjectUpdatedEventArgs (params object[] updatedSubjects)
		{
			UpdatedSubjects = updatedSubjects;
		}
	}
	
}

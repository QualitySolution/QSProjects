using System;

namespace QSOrmProject.UpdateNotification
{
	public class OrmObjectUpdatedGenericEventArgs<TEntity> : EventArgs
	{
		public TEntity[] UpdatedSubjects { get; private set; }

		public OrmObjectUpdatedGenericEventArgs (params TEntity[] updatedSubjects)
		{
			UpdatedSubjects = updatedSubjects;
		}
	}
}


using System;

namespace QSOrmProject.UpdateNotification
{

	public class OrmObjectUpdatedEventArgs : EventArgs
	{
		public object[] UpdatedSubjects { get; private set; }

		public OrmObjectUpdatedEventArgs (params object[] updatedSubjects)
		{
			UpdatedSubjects = updatedSubjects;
		}
	}
	
}

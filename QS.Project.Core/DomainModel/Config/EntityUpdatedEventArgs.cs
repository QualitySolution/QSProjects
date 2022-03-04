using System;

namespace QS.DomainModel.Config
{
	public class EntityUpdatedEventArgs : EventArgs
	{
		public object[] UpdatedSubjects { get; private set; }

		public EntityUpdatedEventArgs(params object[] updatedSubjects)
		{
			UpdatedSubjects = updatedSubjects;
		}
	}
	
}

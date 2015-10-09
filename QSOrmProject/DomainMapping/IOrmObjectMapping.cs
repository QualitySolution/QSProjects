using System;
using QSOrmProject.UpdateNotification;

namespace QSOrmProject
{
	public interface IOrmObjectMapping
	{
		Type ObjectClass { get;}
		Type DialogClass { get;}
		Type RefFilterClass { get;}
		string[] RefSearchFields { get;}
		string RefColumnMappings { get;}
		bool SimpleDialog { get;}
		string EditPermisionName { get;}
		event EventHandler<OrmObjectUpdatedEventArgs> ObjectUpdated;
		void RaiseObjectUpdated(params object[] updatedSubjects);
	}
}


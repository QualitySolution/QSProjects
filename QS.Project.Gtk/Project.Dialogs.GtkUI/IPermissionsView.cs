using System;
namespace QS.Project.Dialogs.GtkUI
{
	public interface IPermissionsView
	{
		string ViewName { get; }

		string DBFieldName { get; }
		string DBFieldValue { get; set; }
	}
}

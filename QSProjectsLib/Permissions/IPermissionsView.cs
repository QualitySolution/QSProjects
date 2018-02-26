using System;
namespace QSProjectsLib.Permissions
{
	public interface IPermissionsView
	{
		string ViewName { get; }

		string DBFieldName { get; }
		string DBFieldValue { get; set; }
	}
}

using System;

namespace QS.Project.Dialogs
{
	[Flags]
	public enum ReferenceButtonMode
	{
		None = 0,
		CanAdd = 1,
		CanEdit = 2,
		CanDelete = 4,
		CanAll = 7,
		TreatEditAsOpen = 8
	}
}
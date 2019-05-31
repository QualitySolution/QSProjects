using System;
namespace QS.RepresentationModel.GtkUI
{
	public interface IJournalPopupItem
	{
		string Title { get; }
		Func<object[], bool> VisibilityFunc { get; }
		Func<object[], bool> SensitivityFunc { get; }
		Action<object[]> ExecuteAction { get; }
	}
}

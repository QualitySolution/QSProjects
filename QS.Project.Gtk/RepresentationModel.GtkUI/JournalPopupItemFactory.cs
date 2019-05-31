using System;
namespace QS.RepresentationModel.GtkUI
{
	public static class JournalPopupItemFactory
	{
		public static IJournalPopupItem CreateNewAlwaysSensitiveAndVisible(string title, Action<object[]> executeAction)
		{
			return CreateNew(title, executeAction, (selected) => true, (selected) => true);
		}

		public static IJournalPopupItem CreateNewAlwaysSensitive(string title, Action<object[]> executeAction, Func<object[], bool> visibilityFunc)
		{
			return CreateNew(title, executeAction, visibilityFunc, (selected) => true);
		}

		public static IJournalPopupItem CreateNewAlwaysVisible(string title, Action<object[]> executeAction, Func<object[], bool> sensitivityFunc)
		{
			return CreateNew(title, executeAction, (selected) => true, sensitivityFunc);
		}

		public static IJournalPopupItem CreateNew(string title, Action<object[]> executeAction, Func<object[], bool> visibilityFunc, Func<object[], bool> sensitivityFunc)
		{
			return new JournalPopupItem(title, executeAction, visibilityFunc, sensitivityFunc);
		}
	}

	public class JournalPopupItem : IJournalPopupItem
	{
		public string Title { get; private set; }

		public Func<object[], bool> VisibilityFunc { get; private set; }

		public Func<object[], bool> SensitivityFunc { get; private set; }

		public Action<object[]> ExecuteAction { get; private set; }

		public JournalPopupItem(string title, Action<object[]> executeAction, Func<object[], bool> visibilityFunc, Func<object[], bool> sensitivityFunc)
		{
			Title = title;
			ExecuteAction = executeAction;
			VisibilityFunc = visibilityFunc;
			SensitivityFunc = sensitivityFunc;
		}
	}
}

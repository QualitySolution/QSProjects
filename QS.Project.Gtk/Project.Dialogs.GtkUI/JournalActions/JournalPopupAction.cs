using System;
using Gtk;
using QS.RepresentationModel.GtkUI;
namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class JournalPopupAction : IJournalPopupAction
	{
		private readonly Func<object[], bool> visibilityFunc;
		private readonly Func<object[], bool> sensitivityFunc;
		private readonly System.Action<object[]> action;

		public JournalPopupAction(IJournalPopupItem popupItem) : this(popupItem.Title, popupItem.VisibilityFunc, popupItem.SensitivityFunc, popupItem.ExecuteAction)
		{
		}

		public JournalPopupAction(string title, Func<object[], bool> visibilityFunc, Func<object[], bool> sensitivityFunc, System.Action<object[]> action)
		{
			Title = title;
			this.visibilityFunc = visibilityFunc ?? throw new ArgumentNullException(nameof(visibilityFunc));
			this.sensitivityFunc = sensitivityFunc ?? throw new ArgumentNullException(nameof(sensitivityFunc));
			this.action = action ?? throw new ArgumentNullException(nameof(action));
			MenuItem = new MenuItem(Title);
			MenuItem.Activated += (sender, e) => { Execute(); };
		}

		public MenuItem MenuItem { get; }

		public string Title { get; }

		public bool Sensetive => MenuItem.Sensitive;

		public bool Visible => MenuItem.Visible;

		public bool Sensitive => throw new NotImplementedException();

		public object[] SelectedItems { get; set; }

		public void CheckSensitive(object[] selected)
		{
			MenuItem.Sensitive = sensitivityFunc.Invoke(selected);
		}

		public void CheckVisibility(object[] selected)
		{
			MenuItem.Visible = visibilityFunc.Invoke(selected);
		}

		public void Execute()
		{
			action.Invoke(SelectedItems);
		}
	}
}

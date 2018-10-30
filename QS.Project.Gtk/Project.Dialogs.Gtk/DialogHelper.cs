using System;
using Gtk;

namespace QS.Project.Dialogs.Gtk
{
	public static class DialogHelper
	{
		public static IEntityDialog FindParentDialog(Widget child)
		{
			if(child.Parent == null)
				return null;
			else if(child.Parent is IEntityDialog)
				return child.Parent as IEntityDialog;
			else if(child.Parent.IsTopLevel)
				return null;
			else
				return FindParentDialog(child.Parent);
		}
	}
}

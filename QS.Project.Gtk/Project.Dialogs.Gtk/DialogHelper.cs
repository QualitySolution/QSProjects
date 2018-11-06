using System;
using Gtk;

namespace QS.Project.Dialogs.Gtk
{
	public static class DialogHelper
	{
		public static IEntityDialog FindParentEntityDialog(Widget child)
		{
			if(child.Parent == null)
				return null;
			else if(child.Parent is IEntityDialog)
				return child.Parent as IEntityDialog;
			else if(child.Parent.IsTopLevel)
				return null;
			else
				return FindParentEntityDialog(child.Parent);
		}

		public static ISingleUoWDialog FindParentUowDialog(Widget child)
		{
			if(child.Parent == null)
				return null;
			else if(child.Parent is ISingleUoWDialog)
				return child.Parent as ISingleUoWDialog;
			else if(child.Parent.IsTopLevel)
				return null;
			else
				return FindParentUowDialog(child.Parent);
		}
	}
}

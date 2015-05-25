using System;
using Gtk;
using NLog;

namespace QSOrmProject.Deletion
{
	public partial class DeleteDlg : Gtk.Dialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public DeleteDlg (TreeStore objectsTreeStore)
		{
			this.Build ();

			treeviewObjects.AppendColumn ("Объект", new Gtk.CellRendererText (), "text", 0);

			treeviewObjects.Model = objectsTreeStore;
			treeviewObjects.ShowAll ();
		}
			
		internal static bool RunDialog (DeleteCore core)
		{
			bool answer;
			if (core.CountReferenceItems > 0) {
				var fullDlg = new DeleteDlg(core.ObjectsTreeStore);
				if (core.CountReferenceItems < 10)
					fullDlg.treeviewObjects.ExpandAll ();
				fullDlg.Title = String.Format ("Удалить {0}?", core.DeletedItems [0].Title);
				answer = (ResponseType)fullDlg.Run () == ResponseType.Yes;
				fullDlg.Destroy();
			} else {
				MessageDialog md = new MessageDialog (null, DialogFlags.DestroyWithParent,
					MessageType.Question, 
					ButtonsType.YesNo, "Вы уверены что хотите удалить <b>" + core.DeletedItems [0].Title + "</b>?");
				answer = (ResponseType)md.Run () == ResponseType.Yes;
				md.Destroy();
			}
			return answer;
		}
	}
}


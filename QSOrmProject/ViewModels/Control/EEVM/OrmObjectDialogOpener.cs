using System;
using Gtk;
using QS.Project.Dialogs.GtkUI;
using QS.Tdi;
using QSOrmProject;

namespace QS.ViewModels.Control.EEVM
{
	public class OrmObjectDialogOpener<TEntity> : IEntityDlgOpener
	{
		readonly ITdiTab MyTab;

		public OrmObjectDialogOpener(ITdiTab parrentTab)
		{
			MyTab = parrentTab ?? throw new ArgumentNullException(nameof(parrentTab));
		}

		public void OpenEntityDlg(int id)
		{
			if(OrmMain.GetObjectDescription(typeof(TEntity)).SimpleDialog) {
				EntityEditSimpleDialog.RunSimpleDialog((MyTab.TabParent as Widget).Toplevel as Window, typeof(TEntity), id);
				return;
			}

			ITdiTab dlg = OrmMain.CreateObjectDialog(typeof(TEntity), id);
			MyTab.TabParent.AddTab(dlg, MyTab);
		}
	}
}

using System;
using Gtk;
using QS.Project.Dialogs.GtkUI;
using QS.Tdi;
using QSOrmProject;

namespace QS.ViewModels.Control.EEVM
{
	public class OrmObjectDialogOpener<TEntity> : IEntityDlgOpener
	{
		readonly Func<ITdiTab> GetMyTab;

		public OrmObjectDialogOpener(Func<ITdiTab> getParrentTab)
		{
			GetMyTab = getParrentTab ?? throw new ArgumentNullException(nameof(getParrentTab));
		}

		public void OpenEntityDlg(int id)
		{
			if(OrmMain.GetObjectDescription(typeof(TEntity)).SimpleDialog) {
				EntityEditSimpleDialog.RunSimpleDialog((GetMyTab().TabParent as Widget).Toplevel as Window, typeof(TEntity), id);
				return;
			}

			ITdiTab dlg = OrmMain.CreateObjectDialog(typeof(TEntity), id);
			GetMyTab().TabParent.AddTab(dlg, GetMyTab());
		}
	}
}

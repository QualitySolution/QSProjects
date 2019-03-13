using System;
using System.Linq;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.Config;
using QS.DomainModel.Entity;
using QS.RepresentationModel.GtkUI;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class RepresentationEditButton : RepresentationButtonBase
	{
		public RepresentationEditButton(IJournalDialog dialog, IRepresentationModel representationModel) 
			: base(dialog, representationModel, "Изменить", "gtk-edit")
		{
		}

		public override void CheckSensitive(object[] selected)
		{
			button.Sensitive = selected.Length == 1;
		}

		public override void Execute()
		{
			var description = DomainConfiguration.GetEntityConfig(RepresentationModel.EntityType);
			if(description == null) {
				throw new NotImplementedException($"Для класса {RepresentationModel.EntityType} отсутствует {typeof(IEntityConfig)}");
			}
			if(description.SimpleDialog) {
				throw new NotImplementedException();
				//OrmSimpleDialog.RunSimpleDialog (this.Toplevel as Window, objectType, datatreeviewRef.GetSelectedObjects () [0]);
			} else {
				int selectedId = DomainHelper.GetId(dialog.SelectedNodes.First());
				dialog.TabParent.OpenTab(DialogHelper.GenerateDialogHashName(RepresentationModel.EntityType, selectedId),
					() => description.CreateDialog(selectedId),
					dialog
				);
			}
		}
	}
}

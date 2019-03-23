using System;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Config;
using QS.RepresentationModel.GtkUI;
using QS.Tdi;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class RepresentationAddButton : RepresentationButtonBase
	{
		public RepresentationAddButton(IJournalDialog dialog, IRepresentationModel representationModel)
			: base(dialog, representationModel, "Добавить", "gtk-add")
		{
		}

		public override void CheckSensitive(object[] selected)
		{
			button.Sensitive = EntityClass != null;
		}

		public override void Execute()
		{
			dialog.TreeView.Selection.UnselectAll();
			var classDiscript = DomainConfiguration.GetEntityConfig(EntityClass);
			if(classDiscript.SimpleDialog) {
				EntityEditSimpleDialog.RunSimpleDialog((dialog as Widget).Toplevel as Window, EntityClass, null);
			} else if(RepresentationModel is IRepresentationModelWithParent) {
				var dlg = DomainConfiguration.GetEntityConfig(RepresentationModel.EntityType).CreateDialog((RepresentationModel as IRepresentationModelWithParent).GetParent);
				dialog.TabParent.AddTab(dlg, dialog);
				dlg.EntitySaved += dlg_EntitySaved;
			} else {
				var dlg = DomainConfiguration.GetEntityConfig(RepresentationModel.EntityType).CreateDialog();
				dlg.EntitySaved += dlg_EntitySaved;
				dialog.TabParent.AddTab(dlg, dialog, true);
			}
		}

		void dlg_EntitySaved(object sender, EntitySavedEventArgs e)
		{
			if(e.Entity != null && dialog.Mode == JournalSelectMode.Single) {
				if(!MessageDialogHelper.RunQuestionDialog("Выбрать созданный объект и вернуться к предыдущему диалогу?")) {
					return;
				}

				dialog.OnObjectSelected(e.Entity);
			}
		}
	}
}

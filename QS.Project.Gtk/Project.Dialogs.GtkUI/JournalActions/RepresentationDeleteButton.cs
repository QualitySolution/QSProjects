﻿using System.Linq;
using QS.Deletion;
using QS.DomainModel.Entity;
using QS.RepresentationModel.GtkUI;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class RepresentationDeleteButton : RepresentationButtonBase
	{
		public RepresentationDeleteButton(IJournalDialog dialog, IRepresentationModel representationModel) 
			: base(dialog, representationModel, "Удалить", "gtk-remove")
		{
		}

		public override void CheckSensitive(object[] selected)
		{
			button.Sensitive = selected.Length == 1;
		}

		public override void Execute()
		{
			int selectedId = DomainHelper.GetId(dialog.SelectedNodes.First());

			if(DeleteHelper.DeleteEntity(EntityClass, selectedId))
				RepresentationModel.UpdateNodes();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Utilities.Text;
using QS.Validation;

namespace QS.Project.Dialogs.GtkUI
{
	public partial class TypeOfEntityDlg : QS.Dialog.Gtk.EntityDialogBase<TypeOfEntity>
	{
		public TypeOfEntityDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<TypeOfEntity>();
			ConfigureDlg();
		}

		public TypeOfEntityDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<TypeOfEntity>(id);
			ConfigureDlg();
		}

		public TypeOfEntityDlg(TypeOfEntity sub) : this(sub.Id) { }

		void ConfigureDlg()
		{
			IList<Type> items = TypeOfEntityRepository.GetEntityTypesMarkedByEntityPermissionAttribute(Entity.Id == 0);
			//Добавить сущность, если атрибут убран, но право осталось
			if(!Entity.IsActive) {
				var t = TypeOfEntityRepository.GetEntityType(Entity.Type);
				//не добавлять, если удалили сам тип
				if(t != null)
					items.Add(t);
			}
			yentryName.Binding.AddBinding(Entity, e => e.CustomName, w => w.Text).InitializeFromSource();
			ylistcomboboxEntityType.SetRenderTextFunc<Type>(TypeOfEntityRepository.GetRealName);
			ylistcomboboxEntityType.ItemsList = items.OrderBy(TypeOfEntityRepository.GetRealName);
			ylistcomboboxEntityType.SelectedItem = items.FirstOrDefault(i => i?.Name == Entity.Type);
			ylistcomboboxEntityType.ItemSelected += YSpecCmbEntityType_ItemSelected;
			SetControlsAcessibility();
		}

		void YSpecCmbEntityType_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			if(e.SelectedItem is Type) {
				Entity.CustomName = TypeOfEntityRepository.GetRealName(e.SelectedItem as Type).StringToTitleCase();
				Entity.Type = (e.SelectedItem as Type).Name;
			}
		}

		void SetControlsAcessibility()
		{
			ylistcomboboxEntityType.Sensitive = Entity.Id == 0;
			yentryName.Sensitive = buttonSave.Sensitive = Entity.IsActive;
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var valid = new QSValidator<TypeOfEntity>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoWGeneric.Save();
			return true;
		}

		#endregion
	}
}
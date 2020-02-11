using System;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Permissions;
using System.Data.Bindings.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace QS.Widgets.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserPresetPermissionWidget : Bin, IUserPermissionTab
	{
		public IUnitOfWork UoW { get; set; }
		public string Title => "Предустановленные";

		UserBase user;

		public UserPresetPermissionWidget()
		{
			this.Build();
			Sensitive = false;
		}

		PresetUserPermissionModel model;

		public void ConfigureDlg(IUnitOfWork uow, UserBase user)
		{
			UoW = uow;
			this.user = user;
			user.LoadUserPermissions();
			model = new PresetUserPermissionModel(UoW, user);

			ytreeviewAvailablePermissions.ColumnsConfig = ColumnsConfigFactory.Create<PresetUserPermissionSource>()
				.AddColumn("Право").AddTextRenderer(x => x.DisplayName)
				.Finish();
			ytreeviewAvailablePermissions.ItemsDataSource = model.ObservablePermissionsSourceList;

			ytreeviewSelectedPermissions.ColumnsConfig = ColumnsConfigFactory.Create<PresetUserPermission>()
				.AddColumn("Право").AddTextRenderer(x => x.DisplayName)
				.RowCells().AddSetter((CellRenderer cell, PresetUserPermission node) => cell.Sensitive = !node.IsLostPermission)
				.Finish();
			ytreeviewSelectedPermissions.ItemsDataSource = model.ObservablePermissionsList;

			Sensitive = true;
		}

		private void AddPermission()
		{
			var selected = ytreeviewAvailablePermissions.GetSelectedObject() as PresetUserPermissionSource;
			model.AddPermission(selected);
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			AddPermission();
		}

		protected void OnYtreeviewAvailablePermissionsRowActivated(object o, RowActivatedArgs args)
		{
			AddPermission();
		}

		private void DeletePermission()
		{
			var selected = ytreeviewSelectedPermissions.GetSelectedObject() as PresetUserPermission;
			model.DeletePermission(selected);
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			DeletePermission();
		}

		protected void OnYtreeviewSelectedPermissionsRowActivated(object o, RowActivatedArgs args)
		{
			DeletePermission();
		}

		public void Save()
		{
			if(model == null) {
				return;
			}
			model.Save();
		}
	}

	internal sealed class PresetUserPermissionModel
	{
		private IUnitOfWork uow;
		private UserBase user;
		private IList<PresetUserPermission> deletePermissionList = new List<PresetUserPermission>();
		private IList<PresetUserPermission> originalPermissionList;
		private IList<PresetUserPermissionSource> originalPermissionsSourceList;

		public GenericObservableList<PresetUserPermission> ObservablePermissionsList { get; private set; }
		public GenericObservableList<PresetUserPermissionSource> ObservablePermissionsSourceList { get; private set; }

		public PresetUserPermissionModel(IUnitOfWork uow, UserBase user)
		{
			this.user = user;
			this.uow = uow;

			originalPermissionList = UserPermissionRepository.GetUserAllPresetPermissions(uow, user.Id);
			ObservablePermissionsList = new GenericObservableList<PresetUserPermission>(originalPermissionList.ToList());

			originalPermissionsSourceList = PermissionsSettings.PresetPermissions.Values.ToList();

			//убираем типы уже загруженные в права
			foreach(var item in originalPermissionList.Where(x => !x.IsLostPermission)) {
				if(originalPermissionsSourceList.Contains(item.PermissionSource)) {
					originalPermissionsSourceList.Remove(item.PermissionSource);
				}
			}
			ObservablePermissionsSourceList = new GenericObservableList<PresetUserPermissionSource>(originalPermissionsSourceList);
		}

		public void AddPermission(PresetUserPermissionSource permissionSource)
		{
			if(permissionSource == null) {
				return;
			}

			ObservablePermissionsSourceList.Remove(permissionSource);

			PresetUserPermission savedPermission;
			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.PermissionSource == permissionSource);
			if(foundOriginalPermission == null) {
				savedPermission = new PresetUserPermission() {
					User = user,
					PermissionName = permissionSource.Name
				};
				ObservablePermissionsList.Add(savedPermission);
			} else {
				if(deletePermissionList.Contains(foundOriginalPermission)) {
					deletePermissionList.Remove(foundOriginalPermission);
				}
				savedPermission = foundOriginalPermission;
				ObservablePermissionsList.Add(savedPermission);
			}
		}

		public void DeletePermission(PresetUserPermission deletedPermission)
		{
			if(deletedPermission == null) {
				return;
			}
			if(!deletedPermission.IsLostPermission)
				ObservablePermissionsSourceList.Add(deletedPermission.PermissionSource);
			ObservablePermissionsList.Remove(deletedPermission);
			if(deletedPermission.Id != 0) {
				deletePermissionList.Add(deletedPermission);
			}
		}

		public void Save()
		{
			foreach(PresetUserPermission item in ObservablePermissionsList) {
				uow.Save(item);
			}

			foreach(var item in deletePermissionList) {
				uow.Delete(item);
			}
		}

	}
}

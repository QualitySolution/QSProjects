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

namespace QS.Widgets.Gtk
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserPresetPermissionWidget : Bin, ISavablePermissionTab
	{
		public IUnitOfWork UoW { get; set; }

		UserBase user;

		public UserPresetPermissionWidget()
		{
			this.Build();
			Sensitive = false;
		}

		PresetUserPermissionModel model;

		public void ConfigureDlg(IUnitOfWork uow, int userId)
		{
			UoW = uow;
			user = UserRepository.GetUserById(UoW, userId);
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

		public void Save()
		{
			model.Save();
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
	}

	internal sealed class PresetUserPermissionModel
	{
		private IUnitOfWork uow;
		private UserBase user;
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

			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.PermissionSource == permissionSource);
			if(foundOriginalPermission == null) {
				ObservablePermissionsList.Add(new PresetUserPermission() {
					User = user,
					PermissionName = permissionSource.Name
				});
			} else {
				ObservablePermissionsList.Add(foundOriginalPermission);
			}
		}

		public void DeletePermission(PresetUserPermission deletedPermission)
		{
			if(deletedPermission == null) {
				return;
			}
			ObservablePermissionsSourceList.Add(deletedPermission.PermissionSource);
			ObservablePermissionsList.Remove(deletedPermission);
		}

		public void Save()
		{
			foreach(PresetUserPermission item in ObservablePermissionsList) {
				if(originalPermissionList.Contains(item)) {
					originalPermissionList.Remove(item);
				}
				uow.Save(item);
			}

			foreach(PresetUserPermission item in originalPermissionList) {
				uow.Delete(item);
			}
		}
	}
}

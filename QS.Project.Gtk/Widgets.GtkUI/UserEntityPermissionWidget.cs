﻿using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Project.Services.GtkUI;

namespace QS.Widgets.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserEntityPermissionWidget : Bin, IUserPermissionTab
	{
		public IUnitOfWork UoW { get; set; }
		public string Title => "На документы";

		UserBase user;

		public UserEntityPermissionWidget()
		{
			this.Build();
			Sensitive = false;
		}

		EntityUserPermissionModel model;

		public void ConfigureDlg(IUnitOfWork uow, UserBase user)
		{
			UoW = uow;
			this.user = user;

			var permissionExtensionStore = PermissionExtensionSingletonStore.GetInstance();
			permissionlistview.ViewModel = new PermissionListViewModel(new GtkInteractiveService(), permissionExtensionStore);
			model = new EntityUserPermissionModel(UoW, user, permissionlistview.ViewModel);

			ytreeviewEntitiesList.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();

			ytreeviewEntitiesList.ItemsDataSource = model.ObservableTypeOfEntitiesList;

			Sensitive = true;
		}

		private void AddPermission()
		{
			var selected = ytreeviewEntitiesList.GetSelectedObject() as TypeOfEntity;
			model.AddPermission(selected);
		}

		private void OnButtonAddClicked(object sender, EventArgs e)
		{
			AddPermission();
		}

		protected void OnYtreeviewEntitiesListRowActivated(object o, RowActivatedArgs args)
		{
			AddPermission();
		}

		public void Save()
		{
			if(model == null) {
				return;
			}
			model.Save();
		}
	}

	internal sealed class EntityUserPermissionModel
	{
		private IUnitOfWork uow;
		private UserBase user;
		private IList<UserPermissionNode> originalPermissionList = new List<UserPermissionNode>();
		private IList<UserPermissionNode> deletePermissionList = new List<UserPermissionNode>();
		private IList<TypeOfEntity> originalTypeOfEntityList;
		public PermissionListViewModel PermissionListViewModel { get; set; }

		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntityUserPermissionModel(IUnitOfWork uow, UserBase user, PermissionListViewModel permissionListViewModel)
		{
			this.user = user;
			this.uow = uow;
			this.PermissionListViewModel = permissionListViewModel ?? throw new NullReferenceException(nameof(permissionListViewModel));

			var permissionList = UserPermissionRepository.GetUserAllEntityPermissions(uow, user.Id, permissionListViewModel.PermissionExtensionStore);
			PermissionListViewModel.PermissionsList = new GenericObservableList<IPermissionNode>(permissionList.OfType<IPermissionNode>().ToList());
			PermissionListViewModel.PermissionsList.ElementRemoved += (aList, aIdx, aObject) => DeletePermission(aObject as UserPermissionNode);

			originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(uow);
			//убираем типы уже загруженные в права
			foreach(var item in originalPermissionList) {
				if(originalTypeOfEntityList.Contains(item.TypeOfEntity)) {
					originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(originalTypeOfEntityList);
		}

		public void AddPermission(TypeOfEntity entityNode)
		{
			if(entityNode == null) {
				return;
			}

			ObservableTypeOfEntitiesList.Remove(entityNode);

			UserPermissionNode savedPermission;
			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null) {
				savedPermission = new UserPermissionNode();
				savedPermission.EntityUserOnlyPermission = new EntityUserPermission() {
					User = user,
					TypeOfEntity = entityNode
				};
				savedPermission.EntityPermissionExtended = new List<EntityUserPermissionExtended>();
				foreach(var item in PermissionListViewModel.PermissionExtensionStore.PermissionExtensions) {
					var node = new EntityUserPermissionExtended();
					node.User = user;
					node.TypeOfEntity = entityNode;
					node.PermissionId = item.PermissionId;
					savedPermission.EntityPermissionExtended.Add(node);
				}
				savedPermission.TypeOfEntity = entityNode;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			} else {
				if(deletePermissionList.Contains(foundOriginalPermission)) {
					deletePermissionList.Remove(foundOriginalPermission);
				}
				savedPermission = foundOriginalPermission;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			}

		}

		public void DeletePermission(UserPermissionNode deletedPermission)
		{
			if(deletedPermission == null) {
				return;
			}
			ObservableTypeOfEntitiesList.Add(deletedPermission.TypeOfEntity);
			PermissionListViewModel.PermissionsList.Remove(deletedPermission);
			if(deletedPermission.EntityUserOnlyPermission.Id != 0) {
				deletePermissionList.Add(deletedPermission);
			}
		}

		public void Save()
		{
			foreach(EntityUserPermission item in PermissionListViewModel.PermissionsList.Select(x => x.EntityPermission as EntityUserPermission).Where(x => x != null))
			{
				uow.Save(item);
				PermissionListViewModel.SaveExtendedPermissions(uow);
			}

			foreach(var item in deletePermissionList) {
				uow.Delete<EntityUserPermission>(item.EntityPermission as EntityUserPermission);
				foreach(var extendedPermission in item.EntityPermissionExtended)
					uow.Delete(extendedPermission);
			}
		}
	}
}

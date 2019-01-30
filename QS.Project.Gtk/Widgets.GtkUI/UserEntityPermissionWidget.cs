using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;

namespace QS.Widgets.Gtk
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserEntityPermissionWidget : Bin, ISavablePermissionTab
	{
		public IUnitOfWork UoW { get; set; }

		UserBase user;

		public UserEntityPermissionWidget()
		{
			this.Build();
			Sensitive = false;
		}

		EntityUserPermissionModel model;

		public void ConfigureDlg(IUnitOfWork uow, int userId)
		{
			UoW = uow;
			user = UserRepository.GetUserById(UoW, userId);
			model = new EntityUserPermissionModel(UoW, user);

			ytreeviewPermissions.ColumnsConfig = ColumnsConfigFactory.Create<EntityUserPermission>()
				.AddColumn("Документ").AddTextRenderer(x => x.TypeOfEntity.CustomName)
				.AddColumn("Просмотр").AddToggleRenderer(x => x.CanRead).Editing()
				.AddColumn("Создание").AddToggleRenderer(x => x.CanCreate).Editing()
				.AddColumn("Редактирование").AddToggleRenderer(x => x.CanUpdate).Editing()
				.AddColumn("Удаление").AddToggleRenderer(x => x.CanDelete).Editing()
				.Finish();

			ytreeviewPermissions.ItemsDataSource = model.ObservablePermissionsList;

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

		private void DeletePermission()
		{
			var selected = ytreeviewPermissions.GetSelectedObject() as EntityUserPermission;
			model.DeletePermission(selected);
		}

		private void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			DeletePermission();
		}

		protected void OnYtreeviewPermissionsRowActivated(object o, RowActivatedArgs args)
		{
			DeletePermission();
		}

		public void Save()
		{
			model.Save();
		}

	}

	internal sealed class EntityUserPermissionModel
	{
		private IUnitOfWork uow;
		private UserBase user;
		private IList<EntityUserPermission> originalPermissionList;
		private IList<TypeOfEntity> originalTypeOfEntityList;

		public GenericObservableList<EntityUserPermission> ObservablePermissionsList { get; private set; }
		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntityUserPermissionModel(IUnitOfWork uow, UserBase user)
		{
			this.user = user;
			this.uow = uow;

			originalPermissionList = UserPermissionRepository.GetUserAllEntityPermissions(uow, user.Id);
			ObservablePermissionsList = new GenericObservableList<EntityUserPermission>(originalPermissionList.ToList());

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

			var foundOriginalPermission = originalPermissionList.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null) {
				ObservablePermissionsList.Add(new EntityUserPermission() {
					User = user,
					TypeOfEntity = entityNode
				});
			}else {
				ObservablePermissionsList.Add(foundOriginalPermission);
			}
		}

		public void DeletePermission(EntityUserPermission deletedPermission)
		{
			if(deletedPermission == null) {
				return;
			}
			ObservableTypeOfEntitiesList.Add(deletedPermission.TypeOfEntity);
			ObservablePermissionsList.Remove(deletedPermission);
		}

		public void Save()
		{
			foreach(EntityUserPermission item in ObservablePermissionsList) {
				if(originalPermissionList.Contains(item)) {
					originalPermissionList.Remove(item);
				}
				uow.Save(item);
			}

			foreach(EntityUserPermission item in originalPermissionList) {
				uow.Delete(item);
			}
		}
	}
}

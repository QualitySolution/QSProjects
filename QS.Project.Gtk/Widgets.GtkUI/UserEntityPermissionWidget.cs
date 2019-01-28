using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Static;

namespace QS.Widgets.Gtk
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserEntityPermissionWidget : Bin
	{
		public IUnitOfWork UoW { get; set; }

		UserBase user;
		GenericObservableList<EntityUserPermission> observableOrderDepositItems;

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
				.AddColumn("Документ").AddTextRenderer(x => x.EntityDisplayName)
				.AddColumn("Просмотр").AddToggleRenderer(x => x.CanRead).Editing()
				.AddColumn("Создание").AddToggleRenderer(x => x.CanCreate).Editing()
				.AddColumn("Редактирование").AddToggleRenderer(x => x.CanUpdate).Editing()
				.AddColumn("Удаление").AddToggleRenderer(x => x.CanDelete).Editing()
				.Finish();

			ytreeviewPermissions.ItemsDataSource = model.PermissionsList;

			ytreeviewEntitiesList.ColumnsConfig = ColumnsConfigFactory.Create<EntityNode>()
				.AddColumn("Документ").AddTextRenderer(x => x.Name)
				.Finish();

			ytreeviewEntitiesList.ItemsDataSource = model.EntityList;

			Sensitive = true;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewEntitiesList.GetSelectedObject() as EntityNode;
			model.AddPermission(selected);
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewPermissions.GetSelectedObject() as EntityUserPermission;
			model.DeletePermission(selected);
		}

		public void Save()
		{
			model.Save();
		}

	}

	public class EntityUserPermissionModel
	{
		private IUnitOfWork uow;
		private UserBase user;
		private IList<EntityUserPermission> originalPermissions;
		private IList<EntityNode> originalEntityList;

		public EntityUserPermissionModel(IUnitOfWork uow, UserBase user)
		{
			this.user = user;
			this.uow = uow;

			originalPermissions = EntityUserPermissionRepository.GetUserAllPermissions(uow, user.Id);
			PermissionsList = new GenericObservableList<EntityUserPermission>(originalPermissions.ToList());

			var havingEntities = PermissionsList.ToDictionary(x => x.EntityName);

			originalEntityList = GenerateEntitiesList(PermissionsSettings.PermissionsEntityTypes)
				.Where(x => !havingEntities.ContainsKey(x.ClassName)).ToList();
			EntityList = new GenericObservableList<EntityNode>(originalEntityList.ToList());
		}

		private List<EntityNode> GenerateEntitiesList(IEnumerable<Type> types)
		{
			var result = new List<EntityNode>();
			foreach(var item in types) {
				var attr = item.GetCustomAttribute<AppellativeAttribute>();
				result.Add(new EntityNode() {
					ClassName = item.Name,
					Name = attr.Nominative
				});
			}
			return result;
		}

		public GenericObservableList<EntityUserPermission> PermissionsList { get; private set; }
		public GenericObservableList<EntityNode> EntityList { get; private set; }

		public void AddPermission(EntityNode entityNode)
		{
			if(entityNode == null) {
				return;
			}

			EntityList.Remove(entityNode);

			var foundOriginalPermission = originalPermissions.FirstOrDefault(x => x.EntityName == entityNode.ClassName);
			if(foundOriginalPermission == null) {
				PermissionsList.Add(new EntityUserPermission() {
					User = user,
					EntityName = entityNode.ClassName
				});
			}else {
				PermissionsList.Add(foundOriginalPermission);
			}
		}

		public void DeletePermission(EntityUserPermission deletedPermission)
		{
			if(deletedPermission == null) {
				return;
			}
			EntityList.Add(originalEntityList.FirstOrDefault(x => x.ClassName == deletedPermission.EntityName));
			PermissionsList.Remove(deletedPermission);
		}

		public void Save()
		{
			foreach(EntityUserPermission item in PermissionsList) {
				originalPermissions.Remove(item);
				uow.Save(item);
			}

			foreach(EntityUserPermission item in originalPermissions) {
				uow.Delete(item);
			}
		}
	}

	public class EntityNode
	{
		public string ClassName { get; set; }
		public string Name { get; set; }
	}
}

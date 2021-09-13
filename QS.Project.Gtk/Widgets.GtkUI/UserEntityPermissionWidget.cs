using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Repositories;

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
			permissionlistview.ViewModel = new PermissionListViewModel(permissionExtensionStore);
			model = new EntityUserPermissionModel(UoW, user, permissionlistview.ViewModel);

			ytreeviewEntitiesList.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();

			ytreeviewEntitiesList.ItemsDataSource = model.ObservableTypeOfEntitiesList;
			searchDocuments.TextChanged += SearchDocumentsOnTextChanged;

			Sensitive = true;
		}

		private void SearchDocumentsOnTextChanged(object sender, EventArgs e)
		{
			ytreeviewEntitiesList.ItemsDataSource = null;
			model.SearchTypes(searchDocuments.Text);
			ytreeviewEntitiesList.ItemsDataSource = model.ObservableTypeOfEntitiesList;
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
		private IList<UserPermissionNode> deletePermissionList = new List<UserPermissionNode>();
		private List<TypeOfEntity> originalTypeOfEntityList;
		public PermissionListViewModel PermissionListViewModel { get; set; }
		private IList<UserPermissionNode> permissionList;

		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntityUserPermissionModel(IUnitOfWork uow, UserBase user, PermissionListViewModel permissionListViewModel)
		{
			this.user = user;
			this.uow = uow;
			this.PermissionListViewModel = permissionListViewModel ?? throw new NullReferenceException(nameof(permissionListViewModel));

			permissionList = UserPermissionRepository.GetUserAllEntityPermissions(uow, user.Id, permissionListViewModel.PermissionExtensionStore);
			PermissionListViewModel.PermissionsList = new GenericObservableList<IPermissionNode>(permissionList.OfType<IPermissionNode>().ToList());
			PermissionListViewModel.PermissionsList.ElementRemoved += (aList, aIdx, aObject) => DeletePermission(aObject as UserPermissionNode);

			originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(uow).ToList();
			//убираем типы уже загруженные в права
			foreach(var item in permissionList) {
				if(originalTypeOfEntityList.Contains(item.TypeOfEntity)) {
					originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}
			SortTypeOfEntityList();
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(originalTypeOfEntityList);

		}
		
		public void SearchTypes(string searchString)
		{
			originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(uow).ToList();
			//убираем типы уже загруженные в права
			foreach(var item in permissionList) {
				if(originalTypeOfEntityList.Contains(item.TypeOfEntity)) {
					originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}
			SortTypeOfEntityList();
			
			ObservableTypeOfEntitiesList = null;
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(originalTypeOfEntityList);

			if (searchString != "")
			{
				for (int i = 0; i < ObservableTypeOfEntitiesList.Count; i++)
				{
					if (ObservableTypeOfEntitiesList[i].Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) == -1)
					{
						ObservableTypeOfEntitiesList.Remove(ObservableTypeOfEntitiesList[i]);
						i -= 1;
					}
				}
			}
		}

		public void AddPermission(TypeOfEntity entityNode)
		{
			if(entityNode == null) {
				return;
			}

			ObservableTypeOfEntitiesList.Remove(entityNode);

			UserPermissionNode savedPermission;
			var foundOriginalPermission = PermissionListViewModel.PermissionsList.OfType<UserPermissionNode>().FirstOrDefault(x => x.TypeOfEntity == entityNode);
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
			SortTypeOfEntityList();
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

		private void SortTypeOfEntityList()
		{
			if(originalTypeOfEntityList?.FirstOrDefault() == null)
				return;

			originalTypeOfEntityList.Sort((x, y) => 
					string.Compare(x.CustomName ?? x.Type, y.CustomName ?? y.Type));
		}
		
	}
}

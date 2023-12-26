using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.EntityRepositories;
using QS.Journal.GtkUI;
using QS.Project.Domain;
using QS.Project.Repositories;

namespace QS.Widgets.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserEntityPermissionWidget : Bin, IUserPermissionTab
	{
		private Menu _availableEntitiesPopupMenu;
		public IUnitOfWork UoW { get; set; }
		public string Title => "На документы";

		UserBase user;

		public UserEntityPermissionWidget()
		{
			this.Build();
			Sensitive = false;
		}

		public void ConfigureDlg(IUnitOfWork uow, UserBase user)
		{
			UoW = uow;
			this.user = user;

			var permissionExtensionStore = PermissionExtensionSingletonStore.GetInstance();
			permissionlistview.ViewModel = new PermissionListViewModel(permissionExtensionStore);
			Model = new EntityUserPermissionModel(UoW, user, permissionlistview.ViewModel);

			ytreeviewEntitiesList.ColumnsConfig = ColumnsConfigFactory.Create<TypeOfEntity>()
				.AddColumn("Документ").AddTextRenderer(x => x.CustomName)
				.Finish();

			ytreeviewEntitiesList.ItemsDataSource = Model.ObservableTypeOfEntitiesList;
			ytreeviewEntitiesList.Binding
				.AddBinding(Model.PermissionListViewModel, vm => vm.SelectedAvailableEntity, w => w.SelectedRow)
				.InitializeFromSource();
			
			ytreeviewEntitiesList.ButtonReleaseEvent += YtreeviewEntitiesListOnButtonReleaseEvent;
			searchDocuments.TextChanged += SearchDocumentsOnTextChanged;

			CreatePopupMenu();

			Sensitive = true;
		}

		private void CreatePopupMenu() {
			_availableEntitiesPopupMenu = new Menu();
			var availablePresetPermissionItem = new MenuItem("Выгрузить в Эксель");
			availablePresetPermissionItem.Activated +=
				(sender, eventArgs) => Model.PermissionListViewModel.ExportFromAvailablePermissionsCommand.Execute();
			availablePresetPermissionItem.Visible = true;

			_availableEntitiesPopupMenu.Add(availablePresetPermissionItem);
			_availableEntitiesPopupMenu.Show();
		}

		private void YtreeviewEntitiesListOnButtonReleaseEvent(object o, ButtonReleaseEventArgs args) {
			if(args.Event.Button != (uint)GtkMouseButton.Right)
			{
				return;
			}
			
			_availableEntitiesPopupMenu.Popup();
		}

		public EntityUserPermissionModel Model { get; set; }

		private void SearchDocumentsOnTextChanged(object sender, EventArgs e)
		{
			ytreeviewEntitiesList.ItemsDataSource = null;
			Model.SearchTypes(searchDocuments.Text);
			ytreeviewEntitiesList.ItemsDataSource = Model.ObservableTypeOfEntitiesList;
		}

		private void AddPermission()
		{
			var selected = ytreeviewEntitiesList.GetSelectedObject() as TypeOfEntity;
			Model.AddPermission(selected);
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
			if(Model == null) {
				return;
			}
			Model.Save();
		}

		public void UpdateData(IList<UserPermissionNode> newUserPermissions) {
			Model.UpdateData(newUserPermissions);
			ytreeviewEntitiesList.ItemsDataSource = Model.ObservableTypeOfEntitiesList;
			permissionlistview.Redraw();
		}
	}

	public sealed class EntityUserPermissionModel {
		private readonly IUnitOfWork _uow;
		private readonly UserBase _user;
		private readonly IList<UserPermissionNode> _deletePermissionList = new List<UserPermissionNode>();
		private IList<UserPermissionNode> _permissionList;
		private List<TypeOfEntity> _originalTypeOfEntityList;
		public PermissionListViewModel PermissionListViewModel { get; set; }

		public GenericObservableList<TypeOfEntity> ObservableTypeOfEntitiesList { get; private set; }

		public EntityUserPermissionModel(IUnitOfWork uow, UserBase user, PermissionListViewModel permissionListViewModel) {
			_user = user;
			_uow = uow;
			PermissionListViewModel = permissionListViewModel ?? throw new NullReferenceException(nameof(permissionListViewModel));

			_permissionList = UserPermissionSingletonRepository.GetInstance()
				.GetUserAllEntityPermissions(_uow, _user.Id, PermissionListViewModel.PermissionExtensionStore).ToList();
			PermissionListViewModel.PermissionsList =
				new GenericObservableList<IPermissionNode>(_permissionList.OfType<IPermissionNode>().ToList());
			PermissionListViewModel.PermissionsList.ElementRemoved += OnPermissionsListElementRemoved;

			_originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(_uow).ToList();
			//убираем типы уже загруженные в права
			foreach(var item in _permissionList) {
				if(_originalTypeOfEntityList.Contains(item.TypeOfEntity)) {
					_originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}

			SortTypeOfEntityList();
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(_originalTypeOfEntityList);
		}

		private void OnPermissionsListElementRemoved(object aList, int[] aIdx, object aObject) {
			DeletePermission(aObject as UserPermissionNode);
		}

		public void SearchTypes(string searchString) {
			_originalTypeOfEntityList = TypeOfEntityRepository.GetAllSavedTypeOfEntity(_uow).ToList();
			//убираем типы уже загруженные в права
			foreach(var item in _permissionList) {
				if(_originalTypeOfEntityList.Contains(item.TypeOfEntity)) {
					_originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}

			SortTypeOfEntityList();

			ObservableTypeOfEntitiesList = null;
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(_originalTypeOfEntityList);

			if(searchString != "") {
				for(int i = 0; i < ObservableTypeOfEntitiesList.Count; i++) {
					if(ObservableTypeOfEntitiesList[i].Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) == -1) {
						ObservableTypeOfEntitiesList.Remove(ObservableTypeOfEntitiesList[i]);
						i -= 1;
					}
				}
			}
		}

		public void AddPermission(TypeOfEntity entityNode) {
			if(entityNode == null) {
				return;
			}

			ObservableTypeOfEntitiesList.Remove(entityNode);

			UserPermissionNode savedPermission;
			var foundOriginalPermission = PermissionListViewModel.PermissionsList.OfType<UserPermissionNode>()
				.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null) {
				savedPermission = new UserPermissionNode();
				savedPermission.EntityUserOnlyPermission = new EntityUserPermission() {
					User = _user,
					TypeOfEntity = entityNode
				};
				savedPermission.EntityPermissionExtended = new List<EntityUserPermissionExtended>();
				foreach(var item in PermissionListViewModel.PermissionExtensionStore.PermissionExtensions) {
					var node = new EntityUserPermissionExtended();
					node.User = _user;
					node.TypeOfEntity = entityNode;
					node.PermissionId = item.PermissionId;
					savedPermission.EntityPermissionExtended.Add(node);
				}

				savedPermission.TypeOfEntity = entityNode;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			}
			else {
				if(_deletePermissionList.Contains(foundOriginalPermission)) {
					_deletePermissionList.Remove(foundOriginalPermission);
				}

				savedPermission = foundOriginalPermission;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			}
		}

		public void DeletePermission(UserPermissionNode deletedPermission) {
			if(deletedPermission == null) {
				return;
			}

			ObservableTypeOfEntitiesList.Add(deletedPermission.TypeOfEntity);
			PermissionListViewModel.PermissionsList.Remove(deletedPermission);
			if(deletedPermission.EntityUserOnlyPermission.Id != 0) {
				_deletePermissionList.Add(deletedPermission);
			}

			SortTypeOfEntityList();
		}

		public void Save() {
			foreach(EntityUserPermission item in PermissionListViewModel.PermissionsList
						.Select(x => x.EntityPermission as EntityUserPermission).Where(x => x != null)) {
				_uow.Save(item);
				PermissionListViewModel.SaveExtendedPermissions(_uow);
			}

			foreach(var item in _deletePermissionList) {
				_uow.Delete(item.EntityPermission);
				foreach(var extendedPermission in item.EntityPermissionExtended)
					_uow.Delete(extendedPermission);
			}
		}

		public void UpdateData(IList<UserPermissionNode> newUserPermissions) {
			PermissionListViewModel.PermissionsList.ElementRemoved -= OnPermissionsListElementRemoved;
			
			_permissionList = newUserPermissions;
			PermissionListViewModel.PermissionsList =
				new GenericObservableList<IPermissionNode>(_permissionList.OfType<IPermissionNode>().ToList());
			PermissionListViewModel.PermissionsList.ElementRemoved += OnPermissionsListElementRemoved;
			
			//убираем типы уже загруженные в права
			foreach(var item in _permissionList) {
				if(_originalTypeOfEntityList.Contains(item.TypeOfEntity)) {
					_originalTypeOfEntityList.Remove(item.TypeOfEntity);
				}
			}

			SortTypeOfEntityList();
			ObservableTypeOfEntitiesList = new GenericObservableList<TypeOfEntity>(_originalTypeOfEntityList);
		}

		private void SortTypeOfEntityList()
		{
			if(_originalTypeOfEntityList?.FirstOrDefault() == null)
				return;

			_originalTypeOfEntityList.Sort((x, y) => 
					string.Compare(x.CustomName ?? x.Type, y.CustomName ?? y.Type));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.EntityRepositories;
using QS.Extensions.Observable.Collections.List;
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

			ytreeviewEntitiesList.ItemsDataSource = Model.DisplayedAvailablePermissionsToAdd;
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
			Model.SearchTypes(searchDocuments.Text);
		}

		private void AddPermission()
		{
			var selected = ytreeviewEntitiesList.GetSelectedObject() as TypeOfEntity;
			Model.AddPermissionToUser(selected);
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
			permissionlistview.Redraw();
		}
	}

	public sealed class EntityUserPermissionModel {
		private readonly IUnitOfWork _uow;
		private readonly UserBase _user;
		private readonly IList<UserPermissionNode> _permissionsToDelete = new List<UserPermissionNode>();
		private readonly IList<TypeOfEntity> _allCurrentAvailablePermissionsToAdd;
		private IList<UserPermissionNode> _permissionList;
		public PermissionListViewModel PermissionListViewModel { get; set; }

		public IObservableList<TypeOfEntity> DisplayedAvailablePermissionsToAdd { get; }

		public EntityUserPermissionModel(IUnitOfWork uow, UserBase user, PermissionListViewModel permissionListViewModel) {
			_user = user;
			_uow = uow;
			PermissionListViewModel = permissionListViewModel ?? throw new NullReferenceException(nameof(permissionListViewModel));

			_permissionList = UserPermissionSingletonRepository.GetInstance()
				.GetUserAllEntityPermissions(_uow, _user.Id, PermissionListViewModel.PermissionExtensionStore).ToList();
			PermissionListViewModel.PermissionsList =
				new ObservableList<IPermissionNode>(_permissionList.OfType<IPermissionNode>().ToList());
			PermissionListViewModel.PermissionsList.CollectionChanged += OnPermissionsListElementRemoved;

			_allCurrentAvailablePermissionsToAdd =
				TypeOfEntityRepository.GetAllSavedTypeOfEntityOrderedByName(_uow);
			
			//убираем типы уже загруженные в права
			foreach(var item in _permissionList) {
				if(_allCurrentAvailablePermissionsToAdd.Contains(item.TypeOfEntity)) {
					_allCurrentAvailablePermissionsToAdd.Remove(item.TypeOfEntity);
				}
			}

			DisplayedAvailablePermissionsToAdd = new ObservableList<TypeOfEntity>(_allCurrentAvailablePermissionsToAdd);
		}

		private void OnPermissionsListElementRemoved(object aList, NotifyCollectionChangedEventArgs args) {
			if(args.Action == NotifyCollectionChangedAction.Remove) {
				foreach(UserPermissionNode item in args.OldItems) {
					RemovePermissionFromUser(item);					
				}
			}
		}

		public void SearchTypes(string searchString) {

			DisplayedAvailablePermissionsToAdd.Clear();

			foreach(var availableTypeOfEntity in _allCurrentAvailablePermissionsToAdd) {
				DisplayedAvailablePermissionsToAdd.Add(availableTypeOfEntity);
			}

			if(searchString != "") {
				for(int i = 0; i < DisplayedAvailablePermissionsToAdd.Count; i++) {
					if(DisplayedAvailablePermissionsToAdd[i].Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) == -1) {
						DisplayedAvailablePermissionsToAdd.Remove(DisplayedAvailablePermissionsToAdd[i]);
						i -= 1;
					}
				}
			}
		}

		public void AddPermissionToUser(TypeOfEntity entityNode) {
			if(entityNode == null) return;

			RemoveAvailablePermission(entityNode);

			UserPermissionNode savedPermission;
			var foundOriginalPermission = PermissionListViewModel.PermissionsList.OfType<UserPermissionNode>()
				.FirstOrDefault(x => x.TypeOfEntity == entityNode);
			if(foundOriginalPermission == null) {
				savedPermission = new UserPermissionNode {
					EntityUserOnlyPermission = new EntityUserPermission {
						User = _user,
						TypeOfEntity = entityNode
					},
					EntityPermissionExtended = new List<EntityUserPermissionExtended>()
				};
				foreach(var item in PermissionListViewModel.PermissionExtensionStore.PermissionExtensions) {
					var node = new EntityUserPermissionExtended {
						User = _user,
						TypeOfEntity = entityNode,
						PermissionId = item.PermissionId
					};
					savedPermission.EntityPermissionExtended.Add(node);
				}

				savedPermission.TypeOfEntity = entityNode;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			}
			else {
				if(_permissionsToDelete.Contains(foundOriginalPermission)) {
					_permissionsToDelete.Remove(foundOriginalPermission);
				}

				savedPermission = foundOriginalPermission;
				PermissionListViewModel.PermissionsList.Add(savedPermission);
			}
		}

		public void Save() {
			foreach(EntityUserPermission item in PermissionListViewModel.PermissionsList
						.Select(x => x.EntityPermission as EntityUserPermission).Where(x => x != null)) {
				_uow.Save(item);
				PermissionListViewModel.SaveExtendedPermissions(_uow);
			}

			foreach(var item in _permissionsToDelete) {
				_uow.Delete<EntityUserPermission>(item.EntityPermission as EntityUserPermission);
				foreach(var extendedPermission in item.EntityPermissionExtended)
					_uow.Delete(extendedPermission);
			}
		}

		public void UpdateData(IList<UserPermissionNode> newUserPermissions) {
			PermissionListViewModel.PermissionsList.CollectionChanged -= OnPermissionsListElementRemoved;
			
			_permissionList = newUserPermissions;
			
			PermissionListViewModel.PermissionsList.Clear();

			foreach(var newPermission in newUserPermissions) {
				PermissionListViewModel.PermissionsList.Add(newPermission);

				if(_allCurrentAvailablePermissionsToAdd.Contains(newPermission.TypeOfEntity)) {
					RemoveAvailablePermission(newPermission.TypeOfEntity);
				}
			}
			
			PermissionListViewModel.PermissionsList.CollectionChanged += OnPermissionsListElementRemoved;
		}

		private void RemoveAvailablePermission(TypeOfEntity entityNode) {
			_allCurrentAvailablePermissionsToAdd.Remove(entityNode);
			DisplayedAvailablePermissionsToAdd.Remove(entityNode);
		}

		private void RemovePermissionFromUser(UserPermissionNode deletedPermission) {
			if(deletedPermission == null) return;

			AddAvailablePermission(deletedPermission.TypeOfEntity);
			
			if(deletedPermission.EntityUserOnlyPermission.Id != 0) {
				_permissionsToDelete.Add(deletedPermission);
			}

			SortTypeOfEntityList();
		}
		
		private void AddAvailablePermission(TypeOfEntity entityNode) {
			_allCurrentAvailablePermissionsToAdd.Add(entityNode);
			DisplayedAvailablePermissionsToAdd.Add(entityNode);
		}

		private void SortTypeOfEntityList()
		{
			if(DisplayedAvailablePermissionsToAdd?.FirstOrDefault() == null) return;
			
			var sortedData = MergeSort(DisplayedAvailablePermissionsToAdd, (x, y) => 
				string.Compare(x.CustomName ?? x.Type, y.CustomName ?? y.Type));
			
			DisplayedAvailablePermissionsToAdd.Clear();
			_allCurrentAvailablePermissionsToAdd.Clear();

			foreach(var typeOfEntity in sortedData) {
				DisplayedAvailablePermissionsToAdd.Add(typeOfEntity);
				_allCurrentAvailablePermissionsToAdd.Add(typeOfEntity);
			}
		}

		private IEnumerable<T> MergeSort<T>(IList<T> data, Comparison<T> comparison) {

			var dataArray = data.ToArray();
			
			MergeSort(dataArray, 0, dataArray.Length - 1, comparison);

			return dataArray;
		}
		
		void MergeSort<T>(T[] array, int left, int right, Comparison<T> comparison) {
			if(left >= right) {
				return;
			}

			var middle = left + (right - left) / 2;

			MergeSort(array, left, middle, comparison);
			MergeSort(array, middle + 1, right, comparison);

			Merge(array, left, middle, right, comparison);
		}
		
		private void Merge<T>(T[] array, int left, int middle, int right, Comparison<T> comparison)
		{
			// Создаем временные массивы
			var leftArrayLength = middle - left + 1;
			var rightArrayLength = right - middle;

			var leftArray = new T[leftArrayLength];
			var rightArray = new T[rightArrayLength];

			// Копируем данные во временные массивы
			Array.Copy(array, left, leftArray, 0, leftArrayLength);
			Array.Copy(array, middle + 1, rightArray, 0, rightArrayLength);

			// Сливаем временные массивы обратно в основной массив
			var i = 0; // Начальный индекс первого подмассива
			var j = 0; // Начальный индекс второго подмассива
			var k = left; // Начальный индекс сливаемого массива

			while(i < leftArrayLength && j < rightArrayLength) {
				var compare = comparison(leftArray[i], rightArray[j]);
				
				if(compare <= 0)
				{
					array[k] = leftArray[i];
					i++;
				}
				else
				{
					array[k] = rightArray[j];
					j++;
				}
				k++;
			}

			// Копируем оставшиеся элементы, если они есть
			while(i < leftArrayLength)
			{
				array[k] = leftArray[i];
				i++;
				k++;
			}

			while(j < rightArrayLength)
			{
				array[k] = rightArray[j];
				j++;
				k++;
			}
		}
	}
}

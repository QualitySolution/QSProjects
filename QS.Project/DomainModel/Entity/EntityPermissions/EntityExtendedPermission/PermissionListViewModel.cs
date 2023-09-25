using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Project.Domain;
using QS.ViewModels;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public class PermissionListViewModel: WidgetViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private TypeOfEntity _selectedAvailableEntity;
		private IPermissionNodeFactory permissionNodeFactory;
		public virtual IPermissionNodeFactory PermissionNodeFactory {
			get => permissionNodeFactory;
			set => SetField(ref permissionNodeFactory, value);
		}

		public PermissionListViewModel(IPermissionExtensionStore permissionExtensionStore)
		{
			PermissionExtensionStore = permissionExtensionStore ?? throw new NullReferenceException(nameof(permissionExtensionStore));
			CreateCommands();
		}

		public TypeOfEntity SelectedAvailableEntity {
			get => _selectedAvailableEntity;
			set => SetField(ref _selectedAvailableEntity, value);
		}

		public IPermissionExtensionStore PermissionExtensionStore { get; set; }
		public Action<(string, string)> ExportAction { get; set; }

		private bool readOnly = false;
		public virtual bool ReadOnly {
			get => readOnly;
			set => SetField(ref readOnly, value, () => ReadOnly);
		}

		private IObservableList<IPermissionNode> permissionsList;
		public virtual IObservableList<IPermissionNode> PermissionsList {
			get => permissionsList;
			set { SetField(ref permissionsList, value, () => PermissionsList);}
		}

		public void SaveExtendedPermissions(IUnitOfWork uow)
		{
			foreach(var item in PermissionsList.SelectMany(x => x.EntityPermissionExtended).Where(x => x.IsPermissionAvailable != null)) 
				uow.Save(item);
			
			foreach(var item in PermissionsList.SelectMany(x => x.EntityPermissionExtended).Where(x => x.IsPermissionAvailable == null && x.Id > 0))
				uow.Delete(item);
		}

		#region Commands

		public DelegateCommand AddItemCommand { get; private set; }
		public DelegateCommand<IPermissionNode> DeleteItemCommand { get; private set; }
		public DelegateCommand ExportFromAvailablePermissionsCommand { get; private set; }
		public DelegateCommand<(string PermissionName, string PermissionTitle)> ExportFromCurrentPermissionsCommand { get; private set; }

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand(
				() => {
					var permissionNode = PermissionNodeFactory?.CreateNode();
					if(permissionNode == null) {
						return;
					}
					if(PermissionsList == null)
						PermissionsList = new ObservableList<IPermissionNode>();
					PermissionsList.Add(permissionNode);
				},
				() => { return !ReadOnly && PermissionNodeFactory != null; }
			);

			DeleteItemCommand = new DelegateCommand<IPermissionNode>(
				(permissionNode) => {
					PermissionsList.Remove(permissionNode);
				},
				(permissionNode) => { return !ReadOnly; }
			);
			
			ExportFromAvailablePermissionsCommand = new DelegateCommand(
				() => {
					ExportAction?.Invoke((SelectedAvailableEntity.Type, SelectedAvailableEntity.Name));
				},
				() => !ReadOnly && SelectedAvailableEntity != null
			);
			
			ExportFromCurrentPermissionsCommand = new DelegateCommand<(string PermissionName, string PermissionTitle)>(
				permission => {
					ExportAction?.Invoke((permission.PermissionName, permission.PermissionTitle));
				},
				permission => !ReadOnly && !string.IsNullOrWhiteSpace(permission.PermissionName)
			);
		}

		#endregion Commands
	}
}

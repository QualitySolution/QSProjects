using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.ViewModels;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public class PermissionListViewModel: WidgetViewModelBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

		public IPermissionExtensionStore PermissionExtensionStore { get; set; }

		private bool readOnly = false;
		public virtual bool ReadOnly {
			get => readOnly;
			set => SetField(ref readOnly, value, () => ReadOnly);
		}

		private GenericObservableList<IPermissionNode> permissionsList;
		public virtual GenericObservableList<IPermissionNode> PermissionsList {
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

		private void CreateCommands()
		{
			AddItemCommand = new DelegateCommand(
				() => {
					var permissionNode = PermissionNodeFactory?.CreateNode();
					if(permissionNode == null) {
						return;
					}
					if(PermissionsList == null)
						PermissionsList = new GenericObservableList<IPermissionNode>();
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
		}

		#endregion Commands
	}
}

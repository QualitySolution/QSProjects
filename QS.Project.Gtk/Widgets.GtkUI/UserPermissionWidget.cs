using System;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Repositories;

namespace QS.Widgets.Gtk
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserPermissionWidget : Bin
	{
		IUnitOfWork UoW;

		public UserPermissionWidget()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
		}

		public void ConfigureDlg()
		{
			userEntityPermissionsView.ConfigureDlg(UoW, UserRepository.GetCurrentUserId());
		}

		public void Save()
		{
			userEntityPermissionsView.Save();
			UoW.Commit();
		}

	}
}

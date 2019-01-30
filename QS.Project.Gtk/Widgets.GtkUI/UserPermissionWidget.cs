using System;
using System.Collections.Generic;
using Gtk;
using QS.DomainModel.UoW;
using QS.Project.Repositories;

namespace QS.Widgets.Gtk
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserPermissionWidget : Bin
	{
		IUnitOfWork UoW;
		List<ISavablePermissionTab> savableTabs = new List<ISavablePermissionTab>();

		public UserPermissionWidget()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
		}

		public void ConfigureDlg()
		{
			var userEntityPermissionWidget = new UserEntityPermissionWidget();
			userEntityPermissionWidget.ConfigureDlg(UoW, UserRepository.GetCurrentUserId());
			AddSavablePermissionTab(userEntityPermissionWidget, "На документы");
			userEntityPermissionWidget.ShowAll();

			var userPresetPermissionWidget = new UserPresetPermissionWidget();
			userPresetPermissionWidget.ConfigureDlg(UoW, UserRepository.GetCurrentUserId());
			AddSavablePermissionTab(userPresetPermissionWidget, "Предустановленные");
			userPresetPermissionWidget.ShowAll();
		}

		public void AddSavablePermissionTab<TPermissionTab>(TPermissionTab permissionTab, string tabName)
			where TPermissionTab : Widget, ISavablePermissionTab
		{
			savableTabs.Add(permissionTab);
			notebook.AppendPage(permissionTab, new Label(tabName));
		}

		public void AddOtherPermissionTab(Widget tabWidget, string tabName)
		{
			notebook.AppendPage(tabWidget, new Label(tabName));
		}

		public void Save()
		{
			foreach(var tab in savableTabs) {
				tab.Save();
			}
			UoW.Commit();
		}
	}
}

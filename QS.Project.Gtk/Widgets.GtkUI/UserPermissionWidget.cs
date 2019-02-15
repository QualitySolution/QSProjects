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

		private List<IUserPermissionTab> userPermissionTabs = new List<IUserPermissionTab>();

		public UserPermissionWidget()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Sensitive = false;
		}

		public void InitilizeTabs()
		{
			var userEntityPermissionWidget = new UserEntityPermissionWidget();
			AddTab(userEntityPermissionWidget);
			userPermissionTabs.Add(userEntityPermissionWidget);

			var userPresetPermissionWidget = new UserPresetPermissionWidget();
			AddTab(userPresetPermissionWidget);
			userPermissionTabs.Add(userPresetPermissionWidget);
		}

		public void ConfigureDlg(int userId)
		{
			if(userId == 0) {
				Sensitive = false;
				return;
			}

			var user = UserRepository.GetUserById(UoW, userId);

			foreach(var tab in userPermissionTabs) {
				tab.ConfigureDlg(UoW, user);
			}

			Sensitive = true;
		}

		public void AddTab(IUserPermissionTab tab)
		{
			var widgetTab = tab as Widget;
			if(widgetTab == null) {
				return;
			}
			notebook.AppendPage(widgetTab, new Label(tab.Title));
			userPermissionTabs.Add(tab);
			widgetTab.ShowAll();
		}

		public void AddTab(Widget tab, string tabName)
		{
			notebook.AppendPage(tab, new Label(tabName));
			tab.ShowAll();
		}

		public void Save()
		{
			foreach(var item in userPermissionTabs) {
				item.Save();
			}
			UoW.Commit();
		}
	}
}

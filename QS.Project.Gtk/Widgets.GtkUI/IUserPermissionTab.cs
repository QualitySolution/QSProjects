using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.Widgets.Gtk
{
	public interface IUserPermissionTab
	{
		string Title { get; }
		void ConfigureDlg(IUnitOfWork uow, UserBase user);
		void Save();
	}
}

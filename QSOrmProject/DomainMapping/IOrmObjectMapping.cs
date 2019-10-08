using System;
using QSOrmProject.UpdateNotification;
using QSOrmProject.DomainMapping;

namespace QSOrmProject
{
	public interface IOrmObjectMapping
	{
		Type ObjectClass { get;}
		Type DialogClass { get;}
		Type RefFilterClass { get;}
		bool? DefaultUseSlider { get;}

		bool SimpleDialog { get;}
		ITableView TableView { get;}
		event EventHandler<OrmObjectUpdatedEventArgs> ObjectUpdated;
		void RaiseObjectUpdated(params object[] updatedSubjects);

		bool PopupMenuExist { get;}
		Gtk.Menu GetPopupMenu(object[] selected);
	}
}


using System;
using System.Collections.Generic;
using System.Collections;
using Gamma.ColumnConfig;

namespace QSOrmProject.RepresentationModel
{
	public interface IRepresentationModel
	{
		Type NodeType { get;}

		Type ObjectType { get;}

		IUnitOfWork UoW { get;}

		IRepresentationFilter RepresentationFilter{ get;}

		IColumnsConfig ColumnsConfig { get;}

		IList ItemsList { get;}

		event EventHandler ItemsListUpdated;

		IEnumerable<string> SearchFields { get;}

		bool SearchFieldsExist { get;}


		string SearchString { get; set;}
		string[] SearchStrings { get; set;}

		void UpdateNodes();

		bool PopupMenuExist { get;}

		Gtk.Menu GetPopupMenu(RepresentationSelectResult[] selected);
	}

	public interface IRepresentationModelWithParent
	{
		object GetParent { get;}
	}
}


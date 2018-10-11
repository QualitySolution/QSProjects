using System;
using System.Collections.Generic;
using System.Collections;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;

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

		bool CanEntryFastSelect { get; }

		bool SearchFilterNodeFunc(object item, string key);

		string SearchString { get; set;}
		string[] SearchStrings { get; set;}

		void UpdateNodes();

		bool PopupMenuExist { get;}

		Gtk.Menu GetPopupMenu(RepresentationSelectResult[] selected);

		void Destroy();
	}

	public interface IRepresentationModelWithParent
	{
		object GetParent { get;}
	}

	public interface INodeWithEntryFastSelect
	{
		string EntityTitle { get; }
	}
}


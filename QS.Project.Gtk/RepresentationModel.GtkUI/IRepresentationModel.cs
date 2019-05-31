using System;
using System.Collections.Generic;
using System.Collections;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;

namespace QS.RepresentationModel.GtkUI
{
	public interface IRepresentationModel
	{
		Type NodeType { get;}

		Type EntityType { get;}

		IUnitOfWork UoW { get;}

		IJournalFilter JournalFilter{ get;}

		IColumnsConfig ColumnsConfig { get;}

		IList ItemsList { get;}

		event EventHandler ItemsListUpdated;

		IEnumerable<string> SearchFields { get;}

		bool SearchFieldsExist { get;}

		bool CanEntryFastSelect { get; }

		bool SearchFilterNodeFunc(object item, string key);

		string SearchString { get; set;}
		string[] SearchStrings { get; set;}

		IEnumerable<IJournalPopupItem> PopupItems { get; }

		void UpdateNodes();

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


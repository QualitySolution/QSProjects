using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using Gamma.Binding;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;

namespace QS.RepresentationModel.GtkUI
{
	[Obsolete("Осталось только для ВОДОВОЗА. Данный интерфейс и классы с ними связанные должны быть удалены или перенесены в OrmProject.")]
	public interface IRepresentationModel
	{
		Type NodeType { get; }

		Type EntityType { get; }

		IUnitOfWork UoW { get; }

		IJournalFilter JournalFilter { get; }

		IColumnsConfig ColumnsConfig { get; }

		IList ItemsList { get; }

		IyTreeModel YTreeModel { get; }

		event EventHandler ItemsListUpdated;
		
		event PropertyChangedEventHandler PropertyChanged;

		IEnumerable<string> SearchFields { get; }

		bool SearchFieldsExist { get; }

		bool CanEntryFastSelect { get; }

		bool SearchFilterNodeFunc(object item, string key);

		string SearchString { get; set; }
		string[] SearchStrings { get; set; }

		IEnumerable<IJournalPopupItem> PopupItems { get; }

		void UpdateNodes();

		void Destroy();
	}

	[Obsolete("Осталось только для ВОДОВОЗА. Данный интерфейс и классы с ними связанные должны быть удалены или перенесены в OrmProject.")]
	public interface IRepresentationModelWithParent
	{
		object GetParent { get;}
	}

	[Obsolete("Осталось только для ВОДОВОЗА. Данный интерфейс и классы с ними связанные должны быть удалены или перенесены в OrmProject.")]
	public interface INodeWithEntryFastSelect
	{
		string EntityTitle { get; }
	}
}


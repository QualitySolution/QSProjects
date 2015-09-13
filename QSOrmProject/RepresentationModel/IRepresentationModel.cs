using System;
using System.Collections.Generic;
using Gtk.DataBindings;
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

		IMappingConfig TreeViewConfig { get;}

		IList ItemsList { get;}

		event EventHandler ItemsListUpdated;

		IEnumerable<string> SearchFields { get;}

		void UpdateNodes();
	}

	//Интерфейс создан на веремя переходного периода. Потому нужно будет или вынести IMappingConfig TreeViewConfig в отдельный интерфейс
	//или отказаться о Gtk.DataBinding полностью.
	public interface IRepresentationModelGamma : IRepresentationModel
	{
		IColumnsConfig ColumnsConfig { get;}
	}

	public interface IRepresentationModelWithParent
	{
		object GetParent { get;}
	}
}


using System;
using Gtk.DataBindings;
using QSOrmProject.RepresentationModel;

namespace QSOrmProject
{

	/// <summary>
	/// Оставлено для совместимости.
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public class OrmTableView : DataTreeView
	{

		IRepresentationModel representationModel;
		public IRepresentationModel RepresentationModel {
			get {
				return representationModel;
			}
			set {if (representationModel == value)
					return;
				representationModel = value;
				ColumnMappingConfig = RepresentationModel.TreeViewConfig;
				RepresentationModel.UpdateNodes ();
				ItemsDataSource = RepresentationModel.ItemsList;
			}
		}

		public OrmTableView ()
		{
			
		}

		public int GetSelectedId()
		{
			var node = GetSelectedObjects ()[0];
			return DomainHelper.GetId (node);
		}

		public object GetSelectedNode()
		{
			return GetSelectedObjects ()[0];
		}

	}
}


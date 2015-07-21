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
		bool itemsSelfSet = true;

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
				base.ItemsDataSource = RepresentationModel.ItemsList;
				RepresentationModel.ItemsListUpdated += RepresentationModel_ItemsListUpdated;
			}
		}

		void RepresentationModel_ItemsListUpdated (object sender, EventArgs e)
		{
			if(itemsSelfSet)
				base.ItemsDataSource = RepresentationModel.ItemsList;
		}

		public override object ItemsDataSource {
			get
			{
				return base.ItemsDataSource;
			}
			set
			{
				base.ItemsDataSource = value;
				itemsSelfSet = value == null;
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


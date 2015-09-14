using System;
using QSOrmProject.RepresentationModel;
using Gamma.GtkWidgets;

namespace QSOrmProject
{

	/// <summary>
	/// Оставлено для совместимости.
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public class RepresentationTreeView : yTreeView
	{
		bool itemsSelfSet = true;

		IRepresentationModelGamma representationModel;
		public IRepresentationModelGamma RepresentationModel {
			get {
				return representationModel;
			}
			set {if (representationModel == value)
					return;
				representationModel = value;
				ColumnsConfig = RepresentationModel.ColumnsConfig;
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

		public RepresentationTreeView ()
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


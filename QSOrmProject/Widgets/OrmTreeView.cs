using System;
using QSOrmProject.RepresentationModel;
using Gtk;
using QSProjectsLib;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem (true)]
	public class OrmTableView : NodeView
	{

		IRepresentationModel representationModel;
		public IRepresentationModel RepresentationModel {
			get {
				return representationModel;
			}
			set {if (representationModel == value)
					return;
				representationModel = value;
				PreparedView ();
			}
		}

		public OrmTableView ()
		{
			
		}

		void PreparedView()
		{
			foreach (IColumnInfo column in RepresentationModel.Columns)
			{
				switch(column.Type)
				{
				case ColumnType.Text:
					this.AppendColumn (column.Name, new CellRendererText (), column.GetAttributes ());
					break;
				}
			}

			NodeStore = representationModel.NodeStore;
		}

		public int GetSelectedId()
		{
			var node = NodeSelection.SelectedNode as object;
			var id = DBWorks.IdPropertyOrNull (node);
			if (id == null)
				return -1;
			else
				return (int)id;
		}
	}
}


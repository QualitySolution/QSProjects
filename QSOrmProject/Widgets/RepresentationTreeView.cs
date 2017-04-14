using System;
using QSOrmProject.RepresentationModel;
using Gamma.GtkWidgets;
using System.Linq;

namespace QSOrmProject
{
	[System.ComponentModel.ToolboxItem (true)]
	public class RepresentationTreeView : yTreeView
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		bool itemsSelfSet = true;

		IRepresentationModel representationModel;
		public IRepresentationModel RepresentationModel {
			get {
				return representationModel;
			}
			set {if (representationModel == value)
					return;
				representationModel = value;
				ColumnsConfig = RepresentationModel.ColumnsConfig;
				var startUpdateTime = DateTime.Now;
				RepresentationModel.UpdateNodes ();
				base.ItemsDataSource = RepresentationModel.ItemsList;
				logger.Debug("Данные представления загружены за {0} сек.", (DateTime.Now - startUpdateTime).TotalSeconds);
				RepresentationModel.ItemsListUpdated += RepresentationModel_ItemsListUpdated;
			}
		}

		void RepresentationModel_ItemsListUpdated (object sender, EventArgs e)
		{
			if(itemsSelfSet)
			{
				SearchHighlightTexts = RepresentationModel.SearchStrings;
				base.ItemsDataSource = RepresentationModel.ItemsList;
			}
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

		public int[] GetSelectedIds ()
		{
			return GetSelectedObjects ().Select(x => DomainHelper.GetId(x)).ToArray();
		}

		public object GetSelectedNode()
		{
			return GetSelectedObject ();
		}

		protected override void OnDestroyed()
		{
			logger.Debug("{0} called Destroy()", this.GetType());

			if (RepresentationModel != null)
			{
				RepresentationModel.Destroy();
			}
			base.OnDestroyed();
		}
	}
}


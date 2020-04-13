using System;
using Gamma.Binding;
using QS.Deletion.ViewModels;
using QS.Views.Dialog;

namespace QS.Deletion.Views
{
	public partial class DeletionView : DialogViewBase<DeletionViewModel>
	{
		public DeletionView(DeletionViewModel viewModel) : base(viewModel)
		{
			this.Build();

			var treeConfig = new RecursiveTreeConfig<TreeNode>
				(x => x.Parrent, x => x.Childs);

			ytreeviewObjects.CreateFluentColumnsConfig<TreeNode>()
				.AddColumn("Объект")
				#if DEBUG
				.AddTextRenderer(x => x.Id > 0 ? x.Id.ToString() : String.Empty)
				#endif
				.AddTextRenderer(x => x.Title)
				.AddColumn("Всего")
				.AddTextRenderer(x => x.TotalChildCount > 0 ? x.TotalChildCount.ToString() : String.Empty)
				.Finish();
				
			ytreeviewObjects.YTreeModel = treeConfig.CreateModel(ViewModel.DeletedItems);

			ytreeviewDependence.CreateFluentColumnsConfig<TreeNode>()
				.AddColumn("Объект")
				#if DEBUG
				.AddTextRenderer(x => x.Id > 0 ? x.Id.ToString() : String.Empty)
				#endif
				.AddTextRenderer(x => x.Title)
				.AddColumn("Всего")
				.AddTextRenderer(x => x.TotalChildCount > 0 ? x.TotalChildCount.ToString() : String.Empty)
				.Finish();
				
			ytreeviewDependence.YTreeModel = treeConfig.CreateModel(ViewModel.DependenceTree);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			ViewModel.CancelDeletion();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			ViewModel.RunDetetion();
		}
	}
}

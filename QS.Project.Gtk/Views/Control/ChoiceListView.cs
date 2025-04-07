using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QS.ViewModels.Control;

namespace QS.Views.Control {
	[ToolboxItem(true)]
	[Category("QS.Control")]
	public partial class ChoiceListView : Gtk.Bin 
	{
		private bool destroyed;
		public BindingControler<ChoiceListView> Binding { get; private set; }
		
		public ChoiceListView(){
			this.Build();
			Binding = new BindingControler<ChoiceListView>(this, new Expression<Func<ChoiceListView, object>>[] 
			{
				w => w.Sensitive
			});
		}
		
		private IChoiceListViewModel viewModel;
		public IChoiceListViewModel ViewModel {
			get => viewModel; 
			set {
				viewModel = value;
				CreateTable();
			}
		}

		private void CreateTable(){
			ytreeChoiseEntities.CreateFluentColumnsConfig<SelectedEntity>()
				.AddColumn("☑").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Name).SearchHighlight()
				.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Sensitive = x.Highlighted)
				.Finish();
			ytreeChoiseEntities.ItemsDataSource = ViewModel.Items;
			
			yentrySearch.Changed += delegate {
				ytreeChoiseEntities.SearchHighlightText = yentrySearch.Text;
				ViewModel.SelectLike(yentrySearch.Text);
				ytreeChoiseEntities.YTreeModel.EmitModelChanged();
			};
			ycheckbuttonChooseAll.Sensitive = ycheckbuttonUnChooseAll.Sensitive = ViewModel.Items.Any();
			ycheckbuttonChooseAll.Clicked += (s,e) => ViewModel.SelectAll();
			ycheckbuttonUnChooseAll.Clicked += (s,e) => ViewModel.UnSelectAll();
		}

		public override void Destroy() 
		{
			if (destroyed) 
			{
				return;
			}

			Binding.CleanSources();
			ViewModel = null;
			ytreeChoiseEntities.Dispose();
			base.Destroy();
			
			destroyed = true;
		}
	}
}

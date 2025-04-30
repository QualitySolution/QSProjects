using System.ComponentModel;
using System.Linq;
using Gamma.Binding.Core;
using QS.ViewModels.Control;

namespace QS.Views.Control {
	[ToolboxItem(true)]
	[Category("QS.Control")]
	public partial class ChoiceListView : Gtk.Bin 
	{
		private bool destroyed;
		public BindingControler<ChoiceListView> Binding { get; }
		
		public ChoiceListView(){
			this.Build();
			Binding = new BindingControler<ChoiceListView>(this);
		}
		
		private IChoiceListViewModel viewModel;
		public IChoiceListViewModel ViewModel {
			get => viewModel; 
			set {
				viewModel = value;
				CreateTable();
			}
		}
		
		public bool ShowColumnsTitle => ytreeChoiseEntities.HeadersVisible;

		private void CreateTable(){
			ytreeChoiseEntities.CreateFluentColumnsConfig<ISelectableEntity>()
				.AddColumn("☑").AddToggleRenderer(x => x.Select).Editing()
				.AddColumn("Название").AddTextRenderer(x => x.Label).SearchHighlight()
				.RowCells().AddSetter<Gtk.CellRendererText>((c, x) => c.Sensitive = x.Highlighted)
				.Finish();
			ytreeChoiseEntities.ItemsDataSource = ViewModel.Items;
			
			yentrySearch.Changed += delegate {
				ytreeChoiseEntities.SearchHighlightText = yentrySearch.Text;
				ViewModel.HighlightLike(yentrySearch.Text);
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

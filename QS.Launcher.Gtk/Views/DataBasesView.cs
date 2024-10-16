using System;
using QS.DbManagement;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Views;

namespace QS.Launcher.Views {
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DataBasesView : ViewBase<DataBasesVM> {

		public DataBasesView(DataBasesVM viewModel) : base(viewModel) {
			this.Build();

			treeBases.CreateFluentColumnsConfig<DbInfo>()
				.AddColumn("Имя базы").AddReadOnlyTextRenderer(x => x.Title)
				.AddColumn("Версия").AddTextRenderer(x => x.Version)
				.Finish();
			treeBases.Binding
				.AddSource(ViewModel)
				.AddBinding(v => v.Databases, w => w.ItemsDataSource)
				.AddBinding(v => v.SelectedDatabase, w => w.SelectedRow)
				.InitializeFromSource();
			treeBases.RowActivated += (o, args) => ViewModel.Connect();
		}

		protected void OnButtonBackClicked(object sender, EventArgs e) {
			ViewModel.PreviousPageCommand.Execute(null);
		}

		protected void OnButtonLoginInBaseClicked(object sender, EventArgs e) {
			ViewModel.Connect();
		}
	}
}

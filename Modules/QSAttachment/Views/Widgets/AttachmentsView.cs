using System;
using Gamma.ColumnConfig;
using QS.Attachments.ViewModels.Widgets;
using QS.Views.GtkUI;

namespace QSAttachment.Views.Widgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AttachmentsView : WidgetViewBase<AttachmentsViewModel>
	{
		public AttachmentsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			buttonAdd.Clicked += (sender, args) => ViewModel.AddCommand.Execute();
			buttonScan.Clicked += (sender, args) => ViewModel.ScanCommand.Execute();
			btnOpen.Clicked += (sender, args) => ViewModel.OpenCommand.Execute();
			btnSave.Clicked += (sender, args) => ViewModel.SaveCommand.Execute();
			btnDelete.Clicked += (sender, args) => ViewModel.DeleteCommand.Execute();

			btnOpen.Binding.AddBinding(ViewModel, vm => vm.CanOpen, w => w.Sensitive).InitializeFromSource();
			btnSave.Binding.AddBinding(ViewModel, vm => vm.CanSave, w => w.Sensitive).InitializeFromSource();
			btnDelete.Binding.AddBinding(ViewModel, vm => vm.CanDelete, w => w.Sensitive).InitializeFromSource();
			
			ConfigureTreeFiles();
		}

		private void ConfigureTreeFiles()
		{
			treeFiles.ColumnsConfig = new FluentColumnsConfig<QS.Attachments.Domain.Attachment>()
				.AddColumn("Файл").AddTextRenderer(n => n.FileName)
				.AddColumn("")
				.Finish();

			treeFiles.ItemsDataSource = ViewModel.Attachments;
			treeFiles.Selection.Changed += OnTreeFilesSelectionChanged;
			treeFiles.RowActivated += (sender, args) => ViewModel.OpenCommand.Execute();
		}

		private void OnTreeFilesSelectionChanged(object sender, EventArgs e)
		{
			ViewModel.SelectedAttachment = treeFiles.GetSelectedObject<QS.Attachments.Domain.Attachment>();
		}
	}
}

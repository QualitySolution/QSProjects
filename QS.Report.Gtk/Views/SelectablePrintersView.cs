using QS.Report.Domain;
using QS.Report.ViewModels;
using System;

namespace QS.Report.Views
{
	public partial class SelectablePrintersView : Gtk.Dialog
	{
		private readonly SelectablePrintersViewModel _selectablePrintersViewModel;

		public SelectablePrintersView(SelectablePrintersViewModel selectablePrintersViewModel)
		{
			_selectablePrintersViewModel = selectablePrintersViewModel ?? throw new ArgumentNullException(nameof(selectablePrintersViewModel));
			this.Build();
			ConfigureDialog();
		}

		private void ConfigureDialog()
		{
			ytreeviewPrinters.CreateFluentColumnsConfig<SelectablePrintersViewModel.PrinterNode>()
				.AddColumn("Название").AddTextRenderer(x => x.Printer.Name)
				.AddColumn("Выбран").AddToggleRenderer(x => x.IsChecked).Editing()
				.Finish();

			ytreeviewPrinters.ItemsDataSource = _selectablePrintersViewModel.AllPrintersWithSelected;

			yenumcomboboxPageOrientation.ItemsEnum = typeof(UserPrintSettings.PageOrientationType);
			yenumcomboboxPageOrientation.Binding.AddBinding(_selectablePrintersViewModel.UserPrintSettings, ups => ups.PageOrientation, w => w.SelectedItem).InitializeFromSource();

			yspinbuttonNumberOfCopies.Binding.AddBinding(_selectablePrintersViewModel.UserPrintSettings, ups => ups.NumberOfCopies, w => w.ValueAsUint).InitializeFromSource();

			buttonSaveSettings.Clicked += (sender, args) => _selectablePrintersViewModel.SavePrintSettingsCommand.Execute();

			buttonOpenSettings.Visible = _selectablePrintersViewModel.IsWindowsOs;
			buttonOpenSettings.Clicked += (sender, args) => _selectablePrintersViewModel.OpenPrinterSettingsCommand.Execute(ytreeviewPrinters.GetSelectedObject<SelectablePrintersViewModel.PrinterNode>());
		}
	}
}

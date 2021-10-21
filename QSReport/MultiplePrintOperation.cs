using fyiReporting.RDL;
using fyiReporting.RdlGtkViewer;
using Gtk;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Report.Domain;
using QS.Report.Repository;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Report
{
	public class MultiplePrintOperation
	{
		private Pages _pages;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly UserPrintingRepository _userPrintingRepository;
		private readonly ICommonServices _commonServices;

		public MultiplePrintOperation(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, UserPrintingRepository userPrintingRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_userPrintingRepository = userPrintingRepository ?? throw new ArgumentNullException(nameof(userPrintingRepository));
		}

		private void Print(string printer, UserPrintSettings userPrintSettings, bool isWindowsOs)
		{
			var printOperation = new PrintOperation
			{
				Unit = Unit.Points,
				UseFullPage = true,
				AllowAsync = true,
				PrintSettings = new PrintSettings
				{
					Printer = printer,
					Orientation = (PageOrientation)Enum.Parse(typeof(PageOrientation), userPrintSettings.PageOrientation.ToString()),
					NCopies = (int)userPrintSettings.NumberOfCopies
				}
			};

			printOperation.BeginPrint += HandlePrintBeginPrint;
			printOperation.DrawPage += HandlePrintDrawPage;
			printOperation.EndPrint += HandlePrintEndPrint;

			printOperation.Run(PrintOperationAction.Print, null);

			if (isWindowsOs)
			{
				ShowPrinterQueue(printer);
			}
		}

		private void HandlePrintBeginPrint(object o, BeginPrintArgs args)
		{
			var printing = (PrintOperation)o;
			printing.NPages = _pages.Count;
		}

		private void HandlePrintDrawPage(object o, DrawPageArgs args)
		{
			using (Cairo.Context g = args.Context.CairoContext)
			{
				RenderCairo render = new RenderCairo(g);
				render.RunPage(_pages[args.PageNr]);
			}
		}

		private void HandlePrintEndPrint(object o, EndPrintArgs args)
		{
			var printing = (PrintOperation)o;
			printing.BeginPrint -= HandlePrintBeginPrint;
			printing.DrawPage -= HandlePrintDrawPage;
			printing.EndPrint -= HandlePrintEndPrint;
			printing.Dispose();
		}

		private void ShowPrinterQueue(string printerName)
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
			{
				WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
				FileName = "cmd.exe",
				Arguments = $"/C rundll32.exe printui.dll,PrintUIEntry /o /n \"{printerName}\""
			};
			process.StartInfo = startInfo;
			process.Start();
		}

		public void Run(Pages pages)
		{
			_pages = pages;

			var selectablePrintersViewModel = new SelectablePrintersViewModel(null, _unitOfWorkFactory, _commonServices, _userPrintingRepository);
			var selectablePrintersView = new SelectablePrintersView(selectablePrintersViewModel);
			selectablePrintersView.ShowAll();
			var response = selectablePrintersView.Run();

			if (response == (int)ResponseType.Ok)
			{
				var selectedPrinters = selectablePrintersViewModel.AllPrintersWithSelected.Where(x => x.IsChecked).Select(x => x.Printer.Name);
				foreach (var printer in selectedPrinters)
				{
					Task.Run(() => Print(printer, selectablePrintersViewModel.UserPrintSettings, selectablePrintersViewModel.IsWindowsOs));
				}
			}

			selectablePrintersView.Destroy();
		}
	}
}

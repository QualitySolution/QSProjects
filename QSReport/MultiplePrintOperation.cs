using fyiReporting.RDL;
using fyiReporting.RdlGtkViewer;
using Gtk;
using QS.DomainModel.UoW;
using QS.Report.Domain;
using QS.Report.Repository;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.Services;
using System;
using System.Linq;
using NLog;

namespace QS.Report
{
	public class MultiplePrintOperation
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IUserPrintingRepository _userPrintingRepository;
		private readonly ICommonServices _commonServices;

		private bool _isPrintingInProgress;

		public MultiplePrintOperation(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			IUserPrintingRepository userPrintingRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_userPrintingRepository = userPrintingRepository ?? throw new ArgumentNullException(nameof(userPrintingRepository));
		}

		public void Run(Pages pages)
		{
			if(_isPrintingInProgress)
			{
				return;
			}

			try
			{
				_isPrintingInProgress = true;

				var selectablePrintersViewModel =
					new SelectablePrintersViewModel(null, _unitOfWorkFactory, _commonServices, _userPrintingRepository);
				var selectablePrintersView = new SelectablePrintersView(selectablePrintersViewModel);
				selectablePrintersView.WindowPosition = WindowPosition.CenterAlways;
				selectablePrintersView.ShowAll();
				var response = selectablePrintersView.Run();

				if(response == (int)ResponseType.Ok)
				{
					var selectedPrinters = selectablePrintersViewModel.AllPrintersWithSelected
						.Where(x => x.IsChecked)
						.Select(x => x.Printer.Name);

					selectablePrintersView.Destroy();
					foreach(var printer in selectedPrinters)
					{
						Print(printer, selectablePrintersViewModel.UserPrintSettings, pages, selectablePrintersViewModel.IsWindowsOs);
					}
				}
				else
				{
					selectablePrintersView.Destroy();
				}
			}
			finally
			{
				_isPrintingInProgress = false;
			}
		}

		private static void Print(string printer, UserPrintSettings userPrintSettings, Pages pages, bool isWindowsOs)
		{
			void HandlePrintBeginPrint(object o, BeginPrintArgs args)
			{
				var printing = (PrintOperation)o;
				printing.NPages = pages.Count;
			}

			void HandlePrintDrawPage(object o, DrawPageArgs args)
			{
				using(var g = args.Context.CairoContext)
				using(var render = new RenderCairo(g))
				{
					render.RunPage(pages[args.PageNr]);
				}
			}

			PrintOperation printOperation = null;
			PrintOperationResult result;

			try
			{
				printOperation = new PrintOperation();
				printOperation.Unit = Unit.Points;
				printOperation.UseFullPage = true;
				printOperation.AllowAsync = true;
				printOperation.PrintSettings = new PrintSettings
				{
					Printer = printer,
					Orientation = (PageOrientation)Enum.Parse(typeof(PageOrientation), userPrintSettings.PageOrientation.ToString()),
					NCopies = (int)userPrintSettings.NumberOfCopies
				};

				printOperation.BeginPrint += HandlePrintBeginPrint;
				printOperation.DrawPage += HandlePrintDrawPage;

				result = printOperation.Run(PrintOperationAction.Print, null);
			}
			catch(Exception e) when(e.Message == "Error from StartDoc")
			{
				result = PrintOperationResult.Cancel;
				_logger.Debug("Операция печати отменена");
			}
			finally
			{
				if(printOperation != null)
				{
					printOperation.BeginPrint -= HandlePrintBeginPrint;
					printOperation.DrawPage -= HandlePrintDrawPage;
					printOperation.Dispose();
				}
			}

			if(isWindowsOs && new[] { PrintOperationResult.Apply, PrintOperationResult.InProgress }.Contains(result))
			{
				ShowPrinterQueue(printer);
			}
		}

		private static void ShowPrinterQueue(string printerName)
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
	}
}

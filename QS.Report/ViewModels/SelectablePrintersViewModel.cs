using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report.Domain;
using QS.Report.Repository;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;

namespace QS.Report.ViewModels
{
	public class SelectablePrintersViewModel : DialogViewModelBase, IDisposable
	{
		private DelegateCommand _savePrintSettingsCommand;
		private DelegateCommand<PrinterNode> _openPrinterSettingsCommand;
		private readonly IList<UserSelectedPrinter> _savedUserPrinterList;
		private readonly IUnitOfWork _uow;
		private readonly UserBase _user;

		public SelectablePrintersViewModel(INavigationManager navigation, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, IUserPrintingRepository userPrintingRepository) : base(navigation)
		{
			_uow = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot();

			_user = commonServices.UserService.GetCurrentUser(_uow);

			_savedUserPrinterList = userPrintingRepository.GetUserSelectedPrinters(_uow, _user.Id);

			UserPrintSettings = userPrintingRepository.GetUserPrintSettings(_uow, _user.Id) ?? new UserPrintSettings
			{
				User = _user
			};

			AllPrintersWithSelected = GeneratePrinterList(_savedUserPrinterList);
		}

		private IEnumerable<PrinterNode> GeneratePrinterList(IList<UserSelectedPrinter> savedUserPrinterList)
		{
			var resultPrinterList = new List<PrinterNode>();

			var installedPrinters = PrinterSettings.InstalledPrinters;
			foreach (var installedPrinter in installedPrinters)
			{
				if (resultPrinterList.All(x => x.Printer.Name != installedPrinter.ToString()))
				{
					var savedUserPrinter = savedUserPrinterList.SingleOrDefault(x => x.Name == installedPrinter.ToString());

					resultPrinterList.Add(new PrinterNode
					{
						Printer = savedUserPrinter ?? new UserSelectedPrinter
						{
							Name = installedPrinter.ToString(),
							User = _user
						},
						IsChecked = !(savedUserPrinterList == null || savedUserPrinterList.All(x => x.Name != installedPrinter.ToString()))
					});
				}
			}

			return resultPrinterList.OrderBy(x => x.Printer.Name).ToList();
		}

		#region Commands

		public DelegateCommand SavePrintSettingsCommand =>
			_savePrintSettingsCommand ?? (_savePrintSettingsCommand = new DelegateCommand(() =>
			{
				var selectedPrinters = AllPrintersWithSelected.Where(x => x.IsChecked).Select(x => x.Printer).ToArray();

				var printersToDelete = _savedUserPrinterList.Except(selectedPrinters).ToArray();
				foreach (var printer in printersToDelete)
				{
					var delPrinter = AllPrintersWithSelected.SingleOrDefault(x => x.Printer.Name == printer.Name);

					if(delPrinter != null) {
						delPrinter.Printer = new UserSelectedPrinter {
							Name = printer.Name,
							User = _user
						};
					}

					_savedUserPrinterList.Remove(printer);
					_uow.Delete(printer);
				}

				var printersToAdd = selectedPrinters.Except(_savedUserPrinterList).ToArray();
				foreach (var printer in printersToAdd)
				{
					_savedUserPrinterList.Add(printer);
					_uow.Save(printer);
				}

				_uow.Save(UserPrintSettings);
				_uow.Commit();
			},
				() => true
			));

		public DelegateCommand<PrinterNode> OpenPrinterSettingsCommand =>
			_openPrinterSettingsCommand ?? (_openPrinterSettingsCommand = new DelegateCommand<PrinterNode>((selectedItem) =>
			{
				if (selectedItem != null)
				{
					string printerName = selectedItem.Printer.Name;
					System.Diagnostics.Process process = new System.Diagnostics.Process();
					System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
					{
						WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
						FileName = "cmd.exe",
						Arguments = $"/C rundll32.exe printui.dll,PrintUIEntry /e /n \"{ printerName }\""
					};
					process.StartInfo = startInfo;
					process.Start();
				}
			},
				(selectedItem) => IsWindowsOs
			));

		#endregion

		public IEnumerable<PrinterNode> AllPrintersWithSelected { get; }
		public UserPrintSettings UserPrintSettings { get; }
		public bool IsWindowsOs => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		public class PrinterNode
		{
			public bool IsChecked { get; set; }
			public UserSelectedPrinter Printer { get; set; }
		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}
}

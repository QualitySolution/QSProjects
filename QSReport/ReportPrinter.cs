using System;
using System.IO;
using fyiReporting.RDL;
using Gtk;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.Repository;
using QS.Services;

namespace QSReport
{
	public class ReportPrinter : IReportPrinter
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly IUserPrintingRepository _userPrintingRepository;

		public ReportPrinter(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			IUserPrintingRepository userPrintingRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_userPrintingRepository = userPrintingRepository ?? throw new ArgumentNullException(nameof(userPrintingRepository));
		}

		public void Print(ReportInfo reportInfo)
		{
			var reportPath = reportInfo.GetPath();
			var source = reportInfo.Source ?? File.ReadAllText(reportPath);

			var rdlParser = new RDLParser(source)
			{
				Folder = Path.GetDirectoryName(reportPath),
				OverwriteConnectionString = reportInfo.ConnectionString,
				OverwriteInSubreport = true
			};

			var report = rdlParser.Parse();
			report.RunGetData(reportInfo.Parameters);
			var pages = report.BuildPages();

			switch(reportInfo.PrintType)
			{
				case ReportInfo.PrintingType.Default:
					var orientation = report.PageHeightPoints > report.PageWidthPoints
						? PageOrientation.Portrait
						: PageOrientation.Landscape;
					var defaultPrintOperation = new DefaultPrintOperation();
					defaultPrintOperation.Run(pages, orientation);
					break;
				case ReportInfo.PrintingType.MultiplePrinters:
					var multiplePrintOperation = new MultiplePrintOperation(_unitOfWorkFactory, _commonServices, _userPrintingRepository);
					multiplePrintOperation.Run(pages);
					break;
				default:
					throw new NotSupportedException($"{reportInfo.PrintType} is not supported");
			}
		}
	}
}

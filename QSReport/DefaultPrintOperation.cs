using System;
using fyiReporting.RDL;
using fyiReporting.RdlGtkViewer;
using Gtk;
using NLog;

namespace QSReport
{
	public class DefaultPrintOperation
	{
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
		private Pages _pages;

		public void Run(Pages pages, PageOrientation pageOrientation)
		{
			_pages = pages;
			PrintOperation printOperation = null;

			try
			{
				printOperation = new PrintOperation();
				printOperation.Unit = Unit.Points;
				printOperation.UseFullPage = true;
				printOperation.DefaultPageSetup = new PageSetup();
				printOperation.DefaultPageSetup.Orientation = pageOrientation;

				printOperation.BeginPrint += HandlePrintBeginPrint;
				printOperation.DrawPage += HandlePrintDrawPage;

				printOperation.Run(PrintOperationAction.PrintDialog, null);
			}
			catch(Exception e) when(e.Message == "Error from StartDoc")
			{
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
		}

		private void HandlePrintBeginPrint(object o, BeginPrintArgs args)
		{
			var printOperation = (PrintOperation)o;
			printOperation.NPages = _pages.Count;
		}

		private void HandlePrintDrawPage(object o, DrawPageArgs args)
		{
			using(var context = args.Context.CairoContext)
			{
				var render = new RenderCairo(context);
				render.RunPage(_pages[args.PageNr]);
			}
		}
	}
}

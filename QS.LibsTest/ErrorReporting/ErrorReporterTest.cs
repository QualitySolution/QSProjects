using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Project.VersionControl;

namespace QS.Test.ErrorReporting
{
	[TestFixture(TestOf = typeof(ErrorReporter))]
	public class ErrorReporterTest
	{
		[Test(Description = "Проверяем что действительно не отправим автоматический отчет, если автоматическая отправка отключена в настройках.")]
		public void SendErrorReport_DisableSendAutomatically()
		{
			var sendService = Substitute.For<IErrorReportingService>();
			var parameters = Substitute.For<IErrorReportingParameters>();
			parameters.CanSendAutomatically.Returns(false);
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductName.Returns("Test");
			var interactive = Substitute.For<IInteractiveMessage>();
			var reporter = new ErrorReporter(() => sendService, parameters, appInfo, interactive);
			reporter.AddException(new System.Exception());

			reporter.SendErrorReport(ErrorReportType.Automatic);
			sendService.DidNotReceive().SubmitErrorReport(Arg.Any<ErrorReport>());
			//Для теста самого теста проверяем что при тех же настройках но разрешенной отправки отчет всетаки отправится.
			//Чтобы исключить возможность что он не отправляется по другой причине.
			parameters.CanSendAutomatically.Returns(true);
			reporter.SendErrorReport(ErrorReportType.Automatic);
			sendService.Received().SubmitErrorReport(Arg.Any<ErrorReport>());
		}

		[Test(Description = "Проверяем что действительно не отправим автоматический отчет, если автоматическая отправка отключена в настройках.")]
		public void SendErrorReport_DontSendAutomaticallyAfterManual()
		{
			var sendService = Substitute.For<IErrorReportingService>();
			sendService.SubmitErrorReport(Arg.Any<ErrorReport>()).Returns(true);
			var parameters = Substitute.For<IErrorReportingParameters>();
			parameters.CanSendAutomatically.Returns(true);
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductName.Returns("Test");
			var interactive = Substitute.For<IInteractiveMessage>();

			var reporter = new ErrorReporter(() => sendService, parameters, appInfo, interactive);
			reporter.AddException(new System.Exception());
			reporter.SendErrorReport(ErrorReportType.User);
			reporter.SendErrorReport(ErrorReportType.Automatic);
			sendService.Received(1).SubmitErrorReport(Arg.Any<ErrorReport>());
		}
	}
}

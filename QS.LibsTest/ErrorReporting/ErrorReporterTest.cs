using System;
using NSubstitute;
using NUnit.Framework;
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
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductName.Returns("Test");

			var assembly = System.Reflection.Assembly.GetAssembly(typeof(ErrorReporter));
			var name = assembly.GetName();
			var version = name.Version;
			appInfo.Version.Returns(version);

			var reporter = new ErrorReporter(sendService, appInfo, canSendAutomatically: false);
			reporter.SendErrorReport(new Exception[] { }, ErrorReportType.Automatic, null, null , null);
			sendService.DidNotReceive().SubmitErrorReport(Arg.Any<ErrorReport>());

			//Для теста самого теста проверяем что при тех же настройках но разрешенной отправки отчет всетаки отправится.
			//Чтобы исключить возможность что он не отправляется по другой причине.
			var reporter2 = new ErrorReporter(sendService, appInfo, canSendAutomatically: true);
			reporter2.SendErrorReport(new Exception[] { }, ErrorReportType.Automatic, null, null, null);
			sendService.Received().SubmitErrorReport(Arg.Any<ErrorReport>());
		}
	}
}

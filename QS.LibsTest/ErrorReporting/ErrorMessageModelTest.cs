using System;
using NSubstitute;
using NUnit.Framework;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Project.Domain;

namespace QS.Test.ErrorReporting
{
	[TestFixture(TestOf = typeof(ErrorReporter))]
	public class ErrorMessageModelTest
	{
		[Test(Description = "Проверяем что действительно не отправим автоматический отчет, если уже был отправлен ручной.")]
		public void SendErrorReport_DontSendAutomaticallyAfterManual()
		{
			var interactive = Substitute.For<IInteractiveMessage>();
			interactive.ShowMessage(Arg.Any<ImportanceLevel>(), Arg.Any<string>());

			var reporter = Substitute.For<IErrorReporter>();
			reporter.CanSendAutomatically.Returns(true);
			reporter.SendErrorReport(
				Arg.Any<Exception[]>(), Arg.Any<ErrorReportType>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UserBase>())
				.Returns(true);
			var model = new ErrorMessageModel(reporter, null, null);

			model.Description = "Test";
			model.ErrorReportType = ErrorReportType.User;
			model.SendErrorReport();
			model.ErrorReportType = ErrorReportType.Automatic;
			model.SendErrorReport();

			reporter.Received(1).SendErrorReport(Arg.Any<Exception[]>(), Arg.Any<ErrorReportType>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UserBase>());
		}
	}
}


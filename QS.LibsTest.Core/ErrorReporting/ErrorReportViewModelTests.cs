using System;
using NSubstitute;
using NUnit.Framework;
using QS.ErrorReporting;
using QS.Project.DB;
using QS.Project.Versioning;

namespace QS.Test.ErrorReporting;

[TestFixture]
public class ErrorReportViewModelTests {
	private IApplicationInfo application;
	private ILogService log;
	private IUserInfo user;

	[SetUp]
	public void SetUp() {
		application = Substitute.For<IApplicationInfo>();
		application.ProductCode.Returns((byte)7);
		application.ProductName.Returns("Тест");
		application.Version.Returns(new Version(1, 2));
		log = Substitute.For<ILogService>();
		log.GetLog(Arg.Any<int?>()).Returns("журнал");
		user = Substitute.For<IUserInfo>();
		user.Name.Returns("Пользователь");
		user.Email.Returns("user@example.com");
	}

	[TestCase(false, false, "", "", true)]
	[TestCase(true, false, "", "", false)]
	[TestCase(true, false, "user@example.com", "", true)]
	[TestCase(false, true, "", " ", false)]
	[TestCase(false, true, "", "Описание", true)]
	[TestCase(true, true, "wrong", "Описание", false)]
	public void RequiredFieldsAreIndependent(bool requireEmail, bool requireDescription, string email, string description, bool expected) {
		var model = new ErrorReportViewModel(new Exception("ошибка"), application, user,
			new ErrorReportingSettings(requireEmail, requireDescription, false, 10), log);
		model.Email = email;
		model.Description = description;
		Assert.That(model.CanSend, Is.EqualTo(expected));
	}

	[TestCase(true, true, "", "", "Заполните описание и укажите корректный email.")]
	[TestCase(true, true, "user@example.com", "", "Заполните описание ошибки.")]
	[TestCase(true, true, "invalid", "Описание", "Укажите корректный email для связи.")]
	[TestCase(false, true, "invalid", "Описание", "")]
	[TestCase(true, false, "user@example.com", "", "")]
	public void ValidationMessageIdentifiesMissingRequiredFields(bool requireEmail, bool requireDescription,
		string email, string description, string expected) {
		var model = new ErrorReportViewModel(new Exception(), application, user,
			new ErrorReportingSettings(requireEmail, requireDescription, false, 10), log);
		model.Email = email;
		model.Description = description;
		Assert.That(model.ValidationMessage, Is.EqualTo(expected));
	}

	[TestCase(ReportType.User, null)]
	[TestCase(ReportType.Automatic, 10)]
	public void ReportIncludesAllExceptionsAndUserInput(ReportType type, int? logRows) {
		SubmitErrorRequest sent = null;
		var model = new ErrorReportViewModel(new Exception("первая"), application, user,
			new ErrorReportingSettings(false, false, true, 10), log, null, report => { sent = report; return true; });
		model.AddException(new Exception("вторая"));
		model.Email = "new@example.com";
		model.Description = "Что произошло";
		Assert.That(model.Submit(type), Is.True);
		Assert.Multiple(() => {
			Assert.That(sent.Report.StackTrace, Does.Contain("первая").And.Contain("вторая"));
			Assert.That(sent.Report.UserDescription, Is.EqualTo("Что произошло"));
			Assert.That(sent.User.Email, Is.EqualTo("new@example.com"));
			Assert.That(sent.ReportType, Is.EqualTo(type));
			Assert.That(model.ReportSent, Is.True);
		});
		log.Received(1).GetLog(logRows);
	}

	[Test]
	public void FailedSendCanBeRetried() {
		var count = 0;
		var model = new ErrorReportViewModel(new Exception(), application, user,
			new ErrorReportingSettings(false, false, true, 10), log, null, report => ++count == 2);
		Assert.That(model.Submit(ReportType.User), Is.False);
		Assert.That(model.ReportSent, Is.False);
		Assert.That(model.Submit(ReportType.User), Is.True);
	}
}

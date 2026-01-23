using NSubstitute;
using NUnit.Framework;
using QS.BaseParameters;
using QS.Dialog;
using QS.Project.Services;
using QS.Project.Versioning;
using QS.Updater;

namespace QS.Test.Updater.App {
	[TestFixture(TestOf = typeof(VersionCheckerService))]
	public class ChangeChannelTest {
		[Test(Description = "Тест для проверки автообновлений")]
		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool NotUpdateTest(bool isOffAutoUpdate) {
			var applicationInfo = Substitute.For<IApplicationInfo>();
			var parametersService = Substitute.For<ParametersService>();
			var checkBaseVersion = new CheckBaseVersion(applicationInfo, parametersService);
			var interactive = Substitute.For<IInteractiveMessage>();
			var applicationQuitService = Substitute.For<IApplicationQuitService>();
			var applicationUpdater  = Substitute.For<IAppUpdater>();
			var dbUpdater = Substitute.For<IDBUpdater>();
			var checker = new VersionCheckerService(checkBaseVersion, interactive, applicationQuitService, applicationUpdater, dbUpdater);
			var result = checker.RunUpdate(isOffAutoUpdate);
			return result == null;
		}
	}
}

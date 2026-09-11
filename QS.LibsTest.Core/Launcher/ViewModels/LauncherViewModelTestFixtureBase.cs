using NUnit.Framework;
using QS.DbManagement;

namespace QS.Launcher.Test.ViewModels {
	public abstract class LauncherViewModelTestFixtureBase : LauncherDbTestFixtureBase {
		protected LauncherPagesHarness Pages { get; private set; }

		[OneTimeSetUp]
		public void SetUpSchedulers() => LauncherPagesHarness.UseImmediateSchedulers();

		[SetUp]
		public void SetUpPages() =>
			Pages = new LauncherPagesHarness(new MariaDbConnectionTypeBase(), "Тестовое подключение", TestProductCode);
	}
}

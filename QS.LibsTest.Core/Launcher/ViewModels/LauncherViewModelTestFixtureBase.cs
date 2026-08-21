using NUnit.Framework;
using QS.DbManagement;

namespace QS.Launcher.Test.ViewModels {
	/// <summary>
	/// Основа для тестов «от нажатия кнопки» поверх свободного подключения: живой сервер в контейнере
	/// от <see cref="LauncherDbTestFixtureBase"/> плюс общая сборка страниц <see cref="LauncherPagesHarness"/>.
	/// </summary>
	public abstract class LauncherViewModelTestFixtureBase : LauncherDbTestFixtureBase {
		protected LauncherPagesHarness Pages { get; private set; }

		[OneTimeSetUp]
		public void SetUpSchedulers() => LauncherPagesHarness.UseImmediateSchedulers();

		[SetUp]
		public void SetUpPages() =>
			Pages = new LauncherPagesHarness(new MariaDbConnectionTypeBase(), "Тестовое подключение", TestProductCode);
	}
}

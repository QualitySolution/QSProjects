using NSubstitute;
using NUnit.Framework;
using QS.Updater;

namespace QS.Test.Updater.App {
	[TestFixture(TestOf = typeof(VersionCheckerService))]
	public class ChangeChannelTest {
		[Test(Description = "Тест для проверки автообновлений")]
		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool NotUpdateTest(bool isOffAutoUpdate) {
			var checker = Substitute.For<VersionCheckerService>();
			var result = checker.RunUpdate(isOffAutoUpdate);
			return result == null;
		}
	}
}

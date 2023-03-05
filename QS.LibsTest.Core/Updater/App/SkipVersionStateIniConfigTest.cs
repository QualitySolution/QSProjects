using System;
using NSubstitute;
using NUnit.Framework;
using QS.Configuration;
using QS.Updater.App;

namespace QS.Test.Updater.App {
	[TestFixture(TestOf = typeof(SkipVersionStateIniConfig))]
	public class SkipVersionStateIniConfigTest {
		
		[TestCase("1.2", 1, 2, 0, 0, ExpectedResult = true)]
		[TestCase("1.2", 1, 2, 1, 0, ExpectedResult = false)]
		[TestCase("1.2.2", 1, 0, 0, 0, ExpectedResult = false)]
		[TestCase("2.2.2.2", 2, 2, 2, 2, ExpectedResult = true)]
		[TestCase("1.0", 1, 0, 0, 0, ExpectedResult = true)]
		[TestCase("", 1, 0, 0, 0, ExpectedResult = false)]
		[TestCase(null, 1, 0, 0, 0, ExpectedResult = false)]
		public bool IsSkippedVersion(string configVersion, int major, int minor, int build, int revision) {
			var requestVersion = new Version(major, minor, build, revision);
			var configuration = Substitute.For<IChangeableConfiguration>();
			configuration["AppUpdater:SkipVersion"].Returns(configVersion);

			var skipState = new SkipVersionStateIniConfig(configuration);
			return skipState.IsSkippedVersion(requestVersion);
		}
	}
}

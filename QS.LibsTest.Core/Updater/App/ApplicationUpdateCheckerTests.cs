using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using QS.Configuration;
using QS.Project.Versioning;
using QS.Updater.App;
using QS.Updates;

namespace QS.Test.Updater.App {
	[TestFixture]
	public class ApplicationUpdateCheckerTests {
		[TestCase(UpdateChannel.Current, ReleaseChannel.Current)]
		[TestCase(UpdateChannel.Stable, ReleaseChannel.Stable)]
		public async Task UsesConfiguredChannelWithoutSerial(UpdateChannel channel, ReleaseChannel expectedChannel) {
			var application = Substitute.For<IApplicationInfo>();
			application.ProductCode.Returns((byte)5);
			application.Version.Returns(new Version(1, 0));
			var configuration = Substitute.For<IChangeableConfiguration>();
			configuration["AppUpdater:Channel"].Returns(channel.ToString());
			var releases = Substitute.For<ReleasesService>();
			var response = new CheckForUpdatesResponse();
			releases.CheckForUpdatesAsync(5, "1.0", "", "", expectedChannel, Arg.Any<CancellationToken>()).Returns(Task.FromResult(response));
			var checker = new ApplicationUpdateChecker(releases, application, new UpdateChannelService(configuration), configuration);
			Assert.That(await checker.CheckAsync(CancellationToken.None), Is.SameAs(response));
			configuration.Received().Reload();
			await releases.Received(1).CheckForUpdatesAsync(5, "1.0", "", "", expectedChannel, Arg.Any<CancellationToken>());
		}

		[Test]
		public async Task OffDoesNotContactServer() {
			var configuration = Substitute.For<IChangeableConfiguration>();
			configuration["AppUpdater:Channel"].Returns("Off");
			var releases = Substitute.For<ReleasesService>();
			var checker = new ApplicationUpdateChecker(releases, Substitute.For<IApplicationInfo>(), new UpdateChannelService(configuration), configuration);
			Assert.That((await checker.CheckAsync(CancellationToken.None)).Releases, Is.Empty);
			await releases.DidNotReceiveWithAnyArgs().CheckForUpdatesAsync(0, null, null, null, default, default);
		}
	}
}

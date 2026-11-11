using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using QS.Dialog;
using QS.Launcher;
using QS.Launcher.AppRunner;
using QS.Launcher.Updates;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Project.Versioning;

namespace QS.Test.Launcher {
	[TestFixture]
	public class UpdateOnDatabasePageTests {
		private ILauncherUpdateService updates;
		private DataBasesVM page;

		[SetUp]
		public void SetUp() {
			updates = Substitute.For<ILauncherUpdateService>();
			page = new DataBasesVM(Substitute.For<IAppRunner>(), Substitute.For<IApplicationInfo>(),
				Substitute.For<IInteractiveMessage>(), new LauncherOptions(), updates);
		}

		[TestCase(false, 1)]
		[TestCase(true, 0)]
		public void AvailableUpdate_ShowsBannerAndOpensUnlessSkipped(bool skipped, int expectedDialogs) {
			updates.CheckAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new LauncherUpdateResult { Version = "2.5", IsSkipped = skipped }));
			page.SetPageActive(true);
			Assert.That(page.HasAvailableUpdate, Is.True);
			Assert.That(page.UpdateBannerText, Does.Contain("2.5"));
			updates.Received(expectedDialogs).ShowUpdate();
			page.ShowUpdateCommand.Execute(null);
			updates.Received(expectedDialogs + 1).ShowUpdate();
		}

		[Test]
		public void LauncherWorksWithoutUpdateModule() {
			var services = new ServiceCollection();
			services.AddSingleton(Substitute.For<IAppRunner>());
			services.AddSingleton(Substitute.For<IApplicationInfo>());
			services.AddSingleton(Substitute.For<IInteractiveMessage>());
			services.AddSingleton(new LauncherOptions());
			services.AddSingleton<DataBasesVM>();
			using(var provider = services.BuildServiceProvider()) {
				var pageWithoutUpdates = provider.GetRequiredService<DataBasesVM>();
				pageWithoutUpdates.SetPageActive(true);
				Assert.That(pageWithoutUpdates.HasAvailableUpdate, Is.False);
				pageWithoutUpdates.ShowUpdateCommand.Execute(null);
			}
		}

		[Test]
		public void DisablingUpdates_HidesBanner() {
			updates.CheckAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new LauncherUpdateResult { Version = "2.5", IsSkipped = true }));
			page.SetPageActive(true);
			updates.UpdatesDisabled += Raise.Event<Action>();
			Assert.That(page.HasAvailableUpdate, Is.False);
		}

		[Test]
		public async Task LeavingPage_DoesNotShowLateResult() {
			var response = new TaskCompletionSource<LauncherUpdateResult>();
			updates.CheckAsync(Arg.Any<CancellationToken>()).Returns(response.Task);
			page.SetPageActive(true);
			page.SetPageActive(false);
			response.SetResult(new LauncherUpdateResult { Version = "2.5" });
			await Task.Yield();
			Assert.That(page.HasAvailableUpdate, Is.False);
			updates.DidNotReceive().ShowUpdate();
		}
	}
}

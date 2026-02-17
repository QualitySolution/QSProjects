using System;
using NSubstitute;
using NUnit.Framework;
using QS.Configuration;
using QS.Dialog;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Versioning;
using QS.Updater;
using QS.Updater.App.ViewModels;
using QS.Updates;

namespace QS.Test.Updater.App {
	[TestFixture(TestOf = typeof(NewVersionViewModel))]
	public class NewVersionViewModelTests {
		
		private IApplicationInfo applicationInfo;
		private INavigationManager navigationManager;
		private ISkipVersionState skipVersionState;
		private ModalProgressCreator progressCreator;
		private IGuiDispatcher guiDispatcher;
		private IInteractiveMessage interactiveMessage;
		private IChangeableConfiguration configuration;
		private IDataBaseInfo dataBaseInfo;

		[SetUp]
		public void Setup() {
			applicationInfo = Substitute.For<IApplicationInfo>();
			applicationInfo.Version.Returns(new Version(1, 0, 0));
			
			navigationManager = Substitute.For<INavigationManager>();
			skipVersionState = Substitute.For<ISkipVersionState>();
			progressCreator = Substitute.For<ModalProgressCreator>();
			guiDispatcher = Substitute.For<IGuiDispatcher>();
			interactiveMessage = Substitute.For<IInteractiveMessage>();
			configuration = Substitute.For<IChangeableConfiguration>();
			
			dataBaseInfo = Substitute.For<IDataBaseInfo>();
			dataBaseInfo.Version.Returns(new Version(1, 0, 0));
		}

		[Test(Description = "WillDbChange должен быть пустым, если нет обновлений базы")]
		public void WillDbChange_NoDbUpdates_ReturnsEmpty() {
			// Arrange
			var releases = new[] {
				CreateRelease("1.1.0", DatabaseUpdate.None),
				CreateRelease("1.2.0", DatabaseUpdate.None)
			};
			
			var viewModel = CreateViewModel(releases);
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			
			// Assert
			Assert.IsFalse(result, "Не должно быть видимой информации об обновлении БД");
		}

		[Test(Description = "WillDbChange должен содержать релиз с Required обновлением")]
		public void WillDbChange_WithRequiredUpdate_ReturnsRelease() {
			// Arrange
			var releases = new[] {
				CreateRelease("1.1.0", DatabaseUpdate.Required),
				CreateRelease("1.2.0", DatabaseUpdate.None)
			};
			
			var viewModel = CreateViewModel(releases);
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.IsTrue(result, "Должна быть видна информация об обновлении БД");
			Assert.That(info, Does.Contain("Потребуется провести изменение базы данных"));
			Assert.That(info, Does.Contain("Совместимость"));
			Assert.That(info, Does.Contain("1.1.х"), "Должна отображаться версия в формате Major.Minor");
		}

		[Test(Description = "WillDbChange должен содержать релиз с BreakingChange обновлением")]
		public void WillDbChange_WithBreakingChange_ReturnsRelease() {
			// Arrange
			var releases = new[] {
				CreateRelease("1.1.0", DatabaseUpdate.BreakingChange),
				CreateRelease("1.2.0", DatabaseUpdate.None)
			};
			
			var viewModel = CreateViewModel(releases);
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.IsTrue(result, "Должна быть видна информация об обновлении БД");
			Assert.That(info, Does.Contain("Внимание!"));
			Assert.That(info, Does.Contain("не смогут работать с базой"));
			Assert.That(info, Does.Contain("1.1"), "Должна отображаться версия в формате Major.Minor");
		}

		[Test(Description = "WillDbChange не должен включать релизы, если версия БД больше или равна версии релиза")]
		public void WillDbChange_DbVersionHigher_SkipsRelease() {
			// Arrange
			dataBaseInfo.Version.Returns(new Version(1, 2, 0));
			
			var releases = new[] {
				CreateRelease("1.1.0", DatabaseUpdate.Required),
				CreateRelease("1.2.0", DatabaseUpdate.Required)
			};
			
			var viewModel = CreateViewModel(releases);
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			
			// Assert
			Assert.IsFalse(result, "Релизы с версией меньше или равной версии БД не должны учитываться");
		}

		[Test(Description = "WillDbChange должен учитывать только релизы начиная с выбранного")]
		public void WillDbChange_FromSelectedRelease_OnlyIncludesLaterReleases() {
			// Arrange
			var releases = new[] {
				CreateRelease("1.1.0", DatabaseUpdate.Required, "http://link1"),
				CreateRelease("1.2.0", DatabaseUpdate.BreakingChange, "http://link2"),
				CreateRelease("1.3.0", DatabaseUpdate.None, "http://link3")
			};
			
			var viewModel = CreateViewModel(releases);
			viewModel.SelectedRelease = viewModel.CanSelectedReleases[1]; // Выбираем 1.2.0
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.IsTrue(result, "Должна учитываться только выбранная версия 1.2.0");
			Assert.That(info, Does.Contain("Внимание!"), "Должна показываться информация о BreakingChange из 1.2.0");
		}

		[Test(Description = "WillDbChange должен быть пустым, если dataBaseInfo == null")]
		public void WillDbChange_NoDataBaseInfo_ReturnsEmpty() {
			// Arrange
			var releases = new[] {
				CreateRelease("1.1.0", DatabaseUpdate.Required),
				CreateRelease("1.2.0", DatabaseUpdate.BreakingChange)
			};
			
			var viewModel = new NewVersionViewModel(
				releases,
				applicationInfo,
				navigationManager,
				skipVersionState,
				progressCreator,
				guiDispatcher,
				interactiveMessage,
				configuration,
				checkBaseVersion: null,
				dataBaseInfo: null
			);
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			
			// Assert
			Assert.IsFalse(result, "Без dataBaseInfo не должно быть информации об обновлениях БД");
		}

		[Test(Description = "WillDbChange должен игнорировать релизы с DatabaseUpdate.None")]
		public void WillDbChange_IgnoresNoneUpdates() {
			// Arrange
			var releases = new[] {
				CreateRelease("1.1.0", DatabaseUpdate.None),
				CreateRelease("1.2.0", DatabaseUpdate.None),
				CreateRelease("1.3.0", DatabaseUpdate.Required)
			};
			
			var viewModel = CreateViewModel(releases);
			
			// Act
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.That(info, Does.Contain("Потребуется провести изменение базы данных"));
			Assert.That(info, Does.Not.Contain("Внимание!"));
		}

		#region Helper Methods
		
		private ReleaseInfo CreateRelease(string version, DatabaseUpdate dbUpdate, string installerLink = "http://example.com/installer.exe") {
			return new ReleaseInfo {
				Version = version,
				DatabaseUpdate = dbUpdate,
				InstallerLink = installerLink,
				Date = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
			};
		}

		private NewVersionViewModel CreateViewModel(ReleaseInfo[] releases, IDataBaseInfo dbInfo = null) {
			return new NewVersionViewModel(
				releases,
				applicationInfo,
				navigationManager,
				skipVersionState,
				progressCreator,
				guiDispatcher,
				interactiveMessage,
				configuration,
				checkBaseVersion: null,
				dataBaseInfo: dbInfo ?? this.dataBaseInfo
			);
		}
		
		#endregion
	}
}

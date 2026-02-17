using System;
using System.Linq;
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
				CreateRelease("1.2.0", DatabaseUpdate.None),
				CreateRelease("1.1.0", DatabaseUpdate.None)
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
				CreateRelease("1.2.0", DatabaseUpdate.None),
				CreateRelease("1.1.0", DatabaseUpdate.Required)
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
				CreateRelease("1.1.1", DatabaseUpdate.Required),
				CreateRelease("1.1", DatabaseUpdate.BreakingChange)
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
				CreateRelease("1.2.0", DatabaseUpdate.Required),
				CreateRelease("1.1.0", DatabaseUpdate.Required)
			};
			
			var viewModel = CreateViewModel(releases);
			// Выбираем релиз с версией равной версии БД (первый - самый новый)
			viewModel.SelectedRelease = viewModel.CanSelectedReleases[0]; // 1.2.0
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			
			// Assert
			Assert.IsFalse(result, "Релизы с версией меньше или равной версии БД не должны учитываться");
		}

		[Test(Description = "WillDbChange должен учитывать только релизы начиная с выбранного")]
		public void WillDbChange_FromSelectedRelease_OnlyIncludesLaterReleases() {
			// Arrange
			var releases = new[] {
				CreateRelease("1.3.0", DatabaseUpdate.None, "http://link3"),
				CreateRelease("1.2.0", DatabaseUpdate.BreakingChange, "http://link2"),
				CreateRelease("1.1.0", DatabaseUpdate.Required, "http://link1")
			};
			
			var viewModel = CreateViewModel(releases);
			viewModel.SelectedRelease = viewModel.CanSelectedReleases[1]; // Выбираем 1.2.0
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.IsTrue(result, "Должны учитываться версии начиная с 1.2.0 и старше");
			Assert.That(info, Does.Contain("Внимание!"), "Должна показываться информация о BreakingChange из 1.2.0");
		}

		[Test(Description = "WillDbChange должен быть пустым, если dataBaseInfo == null")]
		public void WillDbChange_NoDataBaseInfo_ReturnsEmpty() {
			// Arrange
			var releases = new[] {
				CreateRelease("1.2.0", DatabaseUpdate.BreakingChange),
				CreateRelease("1.1.0", DatabaseUpdate.Required)
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
				CreateRelease("1.3.0", DatabaseUpdate.Required),
				CreateRelease("1.2.0", DatabaseUpdate.None),
				CreateRelease("1.1.0", DatabaseUpdate.None)
			};
			
			var viewModel = CreateViewModel(releases);
			
			// Act
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.That(info, Does.Contain("Потребуется провести изменение базы данных"));
			Assert.That(info, Does.Not.Contain("Внимание!"));
		}

		[Test(Description = "CanSelectedReleases должен оставлять только первые версии с уникальными ссылками установщика")]
		public void CanSelectedReleases_FiltersUniqueInstallerLinks() {
			// Arrange - создаём релизы где 2.6.2.1 и 2.6.2 имеют одинаковую ссылку (hotfix)
			var sameLink = "http://example.com/installer-2.6.2.exe";
			var releases = new[] {
				CreateRelease("2.6.3", DatabaseUpdate.None), // уникальная ссылка
				CreateRelease("2.6.2.1", DatabaseUpdate.None, sameLink), // та же ссылка что у 2.6.2
				CreateRelease("2.6.2", DatabaseUpdate.None, sameLink), // та же ссылка что у 2.6.2.1
				CreateRelease("2.6.1", DatabaseUpdate.None), // уникальная ссылка
				CreateRelease("2.6.0", DatabaseUpdate.None) // уникальная ссылка
			};
			
			var viewModel = CreateViewModel(releases);
			
			// Act
			var canSelected = viewModel.CanSelectedReleases;
			
			// Assert
			Assert.AreEqual(4, canSelected.Count, "Должно быть 4 релиза: 2.6.3, 2.6.2.1 (первый с общей ссылкой), 2.6.1, 2.6.0");
			Assert.That(canSelected.Select(x => x.Version), Does.Contain("2.6.3"));
			Assert.That(canSelected.Select(x => x.Version), Does.Contain("2.6.2.1"), "Должна быть версия 2.6.2.1 как первая с этой ссылкой");
			Assert.That(canSelected.Select(x => x.Version), Does.Not.Contain("2.6.2"), "Версия 2.6.2 не должна быть в списке, т.к. у неё та же ссылка что у 2.6.2.1");
			Assert.That(canSelected.Select(x => x.Version), Does.Contain("2.6.1"));
			Assert.That(canSelected.Select(x => x.Version), Does.Contain("2.6.0"));
		}

		[Test(Description = "DbUpdateInfo должен показывать предупреждение, если выбранная версия ниже версии базы")]
		public void DbUpdateInfo_SelectedVersionLowerThanDb_ShowsWarning() {
			// Arrange
			dataBaseInfo.Version.Returns(new Version(2, 5, 0));
			
			var releases = new[] {
				CreateRelease("2.6.0", DatabaseUpdate.None),
				CreateRelease("2.4.0", DatabaseUpdate.None),
				CreateRelease("2.3.0", DatabaseUpdate.None)
			};
			
			var viewModel = CreateViewModel(releases);
			viewModel.SelectedRelease = viewModel.CanSelectedReleases[2]; // Выбираем 2.3.0
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.IsTrue(result, "Должно быть видимо предупреждение");
			Assert.That(info, Does.Contain("Версия базы данных (2.5) новее выбранной версии программы"));
			Assert.That(info, Does.Contain("минимум до версии 2.5"));
			Assert.That(info, Does.Contain("не сможете зайти в программу"));
		}

		[Test(Description = "DbUpdateInfo не должен показывать предупреждение, если Major.Minor версий равны (даже если patch версия базы выше)")]
		public void DbUpdateInfo_SelectedVersionEqualToDb_NoWarning() {
			// Arrange
			dataBaseInfo.Version.Returns(new Version(2, 5, 3));
			
			var releases = new[] {
				CreateRelease("2.6.0", DatabaseUpdate.None),
				CreateRelease("2.5.0", DatabaseUpdate.None)
			};
			
			var viewModel = CreateViewModel(releases);
			viewModel.SelectedRelease = viewModel.CanSelectedReleases[1]; // Выбираем 2.5.0
			
			// Act
			var result = viewModel.VisibleDbUpdateInfo;
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.IsFalse(result, "Не должно быть предупреждения, т.к. Major.Minor совпадают (2.5)");
			Assert.IsNull(info);
		}

		[Test(Description = "DbUpdateInfo должен показывать информацию об обновлении, если выбранная версия выше версии базы")]
		public void DbUpdateInfo_SelectedVersionHigherThanDb_ShowsUpdateInfo() {
			// Arrange
			dataBaseInfo.Version.Returns(new Version(2, 5, 0));
			
			var releases = new[] {
				CreateRelease("2.7", DatabaseUpdate.BreakingChange),
				CreateRelease("2.6", DatabaseUpdate.BreakingChange)
			};
			
			var viewModel = CreateViewModel(releases);
			viewModel.SelectedRelease = viewModel.CanSelectedReleases[1]; // Выбираем 2.6.0
			
			// Act
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.That(info, Does.Contain("обновление базы данных"));
			Assert.That(info, Does.Contain("до 2.6"));
		}

		[Test(Description = "DbUpdateInfo должен показывать максимальную версию базы при нескольких обновлениях")]
		public void DbUpdateInfo_MultipleUpdates_ShowsMaxVersion() {
			// Arrange
			dataBaseInfo.Version.Returns(new Version(1, 0, 0));
			
			var releases = new[] {
				CreateRelease("1.4.0", DatabaseUpdate.None),
				CreateRelease("1.3.0", DatabaseUpdate.BreakingChange),
				CreateRelease("1.2.0", DatabaseUpdate.Required),
				CreateRelease("1.1.0", DatabaseUpdate.Required)
			};
			
			var viewModel = CreateViewModel(releases);
			viewModel.SelectedRelease = viewModel.CanSelectedReleases[2]; // Выбираем 1.2
			
			// Act
			var info = viewModel.DbUpdateInfo;
			
			// Assert
			Assert.That(info, Does.Contain("до 1.2.0"), 
				"Должна показываться максимальная версия на которую обновляемся (1.2.0)");
			Assert.That(info, Does.Not.Contain("Внимание!"), "Не должно быть предупреждение о BreakingChange");
		}

		#region Helper Methods
		
		private ReleaseInfo CreateRelease(string version, DatabaseUpdate dbUpdate, string installerLink = null) {
			return new ReleaseInfo {
				Version = version,
				DatabaseUpdate = dbUpdate,
				InstallerLink = installerLink ?? $"http://example.com/installer-{version}.exe",
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

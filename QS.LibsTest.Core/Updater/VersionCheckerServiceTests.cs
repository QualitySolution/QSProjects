using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.BaseParameters;
using QS.Configuration;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Services;
using QS.Project.Versioning;
using QS.Testing.Gui;
using QS.Updater;
using QS.Updater.App;
using QS.Updater.App.ViewModels;
using QS.Updates;
using QS.ViewModels.Dialog;

namespace QS.Test.Updater
{
	/// <summary>
	/// Интеграционные тесты для VersionCheckerService
	/// </summary>
	[TestFixture(TestOf = typeof(VersionCheckerService))]
	public class VersionCheckerServiceTests
	{
		/// <summary>
		/// Тест 1: версия базы 2.8, версия программы 2.8.5, с сервера приходит возможность обновиться на 2.9.1,
		/// ни какая версия не пропущена, программа должна предложить обновление (открыть окно обновления).
		/// </summary>
		[Test(Description = "Проверяем, что окно обновления открывается, когда есть обновление и версия не пропущена")]
		public void RunUpdate_WhenUpdateAvailableAndNotSkipped_ShouldOpenUpdateDialog()
		{
			// Arrange
			var appVersion = new Version(2, 8, 5);
			var updateVersion = new Version(2, 9, 1);

			// Мокаем ParametersService для имитации версии базы
			var parametersService = CreateParametersService("TestProduct", "2.8", "standard");

			// Создаем ApplicationInfo с версией программы 2.8.5
			var applicationInfo = CreateApplicationInfo("TestProduct", appVersion, new[] { "standard" });

			// Создаем CheckBaseVersion
			var checkBaseVersion = new CheckBaseVersion(applicationInfo, parametersService);

			// Мокаем зависимости
			var quitService = Substitute.For<IApplicationQuitService>();
			var dbUpdater = Substitute.For<IDBUpdater>();
			dbUpdater.HasUpdates.Returns(false);

			// Настраиваем конфигурацию без пропуска версии
			var configuration = Substitute.For<IChangeableConfiguration>();
			configuration["AppUpdater:SkipVersion"].Returns((string)null);

			var skipVersionState = new SkipVersionStateIniConfig(configuration);

			// Создаем реальный ApplicationUpdater
			bool dialogOpened = false;
			var releasesService = CreateReleasesService(updateVersion.ToString());
			var navigationManager = CreateNavigationManager(() => dialogOpened = true, CloseSource.Self);
			var interactiveService = Substitute.For<IInteractiveService>();
			var guiDispatcher = new GuiDispatcherForTests();

			var applicationUpdater = new ApplicationUpdater(
				releasesService,
				applicationInfo,
				navigationManager,
				interactiveService,
				guiDispatcher,
				quitService,
				null,
				parametersService
			);

			// Создаем VersionCheckerService
			var versionChecker = new VersionCheckerService(
				checkBaseVersion,
				applicationUpdater,
				dbUpdater,
				skipVersionState
			);

			// Act
			var result = versionChecker.RunUpdate();

			// Assert
			Assert.IsTrue(dialogOpened, "Диалог обновления должен был открыться");
			Assert.IsNotNull(result);
			Assert.AreEqual(UpdateStatus.Shelve, result.Value.Status);
		}

		/// <summary>
		/// Тест 2: все то же самое, версия базы 2.8, программы 2.8.5, с сервера приходит обновление на 2.9.1
		/// при этом в конфиге указано что 2.9.1 мы пропускаем, значит окно с обновлением не должно показаться.
		/// </summary>
		[Test(Description = "Проверяем, что окно обновления не открывается, когда версия пропущена")]
		public void RunUpdate_WhenUpdateSkippedInConfig_ShouldNotOpenUpdateDialog()
		{
			// Arrange
			var appVersion = new Version(2, 8, 5);
			var updateVersion = new Version(2, 9, 1);

			// Мокаем ParametersService для имитации версии базы
			var parametersService = CreateParametersService("TestProduct", "2.8", "standard");

			// Создаем ApplicationInfo с версией программы 2.8.5
			var applicationInfo = CreateApplicationInfo("TestProduct", appVersion, new[] { "standard" });

			// Создаем CheckBaseVersion
			var checkBaseVersion = new CheckBaseVersion(applicationInfo, parametersService);

			// Мокаем зависимости
			var quitService = Substitute.For<IApplicationQuitService>();
			var dbUpdater = Substitute.For<IDBUpdater>();
			dbUpdater.HasUpdates.Returns(false);

			// Настраиваем конфигурацию с пропуском версии 2.9.1
			var configuration = Substitute.For<IChangeableConfiguration>();
			configuration["AppUpdater:SkipVersion"].Returns("2.9.1");

			var skipVersionState = new SkipVersionStateIniConfig(configuration);

			// Создаем реальный ApplicationUpdater
			bool dialogOpened = false;
			var releasesService = CreateReleasesService(updateVersion.ToString());
			var navigationManager = CreateNavigationManager(() => dialogOpened = true, CloseSource.Self);
			var interactiveService = Substitute.For<IInteractiveService>();
			var guiDispatcher = new GuiDispatcherForTests();

			var applicationUpdater = new ApplicationUpdater(
				releasesService,
				applicationInfo,
				navigationManager,
				interactiveService,
				guiDispatcher,
				quitService,
				null,
				parametersService
			);

			// Создаем VersionCheckerService
			var versionChecker = new VersionCheckerService(
				checkBaseVersion,
				applicationUpdater,
				dbUpdater,
				skipVersionState
			);

			// Act
			var result = versionChecker.RunUpdate();

			// Assert
			Assert.IsFalse(dialogOpened, "Диалог обновления НЕ должен был открыться, так как версия пропущена");
			Assert.IsNotNull(result);
			Assert.AreEqual(UpdateStatus.Skip, result.Value.Status);
		}

		/// <summary>
		/// Тест 3: Теперь у нас версия базы 2.9, программы так же 2.8.5, с сервера так же прилетает обновление на 2.9.1
		/// но у нас в конфиге стоит пропустить 2.9.1. Но так как база уже обновлена, мы должны проигнорировать
		/// пропуск версии и все же предложить обновление (чтобы диалог показался).
		/// </summary>
		[Test(Description = "Проверяем, что окно обновления открывается, когда база обновлена, даже если версия пропущена")]
		public void RunUpdate_WhenDatabaseVersionGreaterAndUpdateSkipped_ShouldOpenUpdateDialog()
		{
			// Arrange
			var appVersion = new Version(2, 8, 5);
			var updateVersion = new Version(2, 9, 1);

			// Мокаем ParametersService для имитации версии базы 2.9
			var parametersService = CreateParametersService("TestProduct", "2.9", "standard");

			// Создаем ApplicationInfo с версией программы 2.8.5
			var applicationInfo = CreateApplicationInfo("TestProduct", appVersion, new[] { "standard" });

			// Создаем CheckBaseVersion
			var checkBaseVersion = new CheckBaseVersion(applicationInfo, parametersService);

			// Мокаем зависимости
			var quitService = Substitute.For<IApplicationQuitService>();
			var dbUpdater = Substitute.For<IDBUpdater>();
			dbUpdater.HasUpdates.Returns(false);

			// Настраиваем конфигурацию с пропуском версии 2.9.1
			var configuration = Substitute.For<IChangeableConfiguration>();
			configuration["AppUpdater:SkipVersion"].Returns("2.9.1");

			var skipVersionState = new SkipVersionStateIniConfig(configuration);

			// Создаем реальный ApplicationUpdater
			bool dialogOpened = false;
			var releasesService = CreateReleasesService(updateVersion.ToString());
			var navigationManager = CreateNavigationManager(() => dialogOpened = true, CloseSource.Self);
			var interactiveService = Substitute.For<IInteractiveService>();
			interactiveService.Question(Arg.Any<string[]>(), Arg.Any<string>()).Returns("");
			var guiDispatcher = new GuiDispatcherForTests();

			var applicationUpdater = new ApplicationUpdater(
				releasesService,
				applicationInfo,
				navigationManager,
				interactiveService,
				guiDispatcher,
				quitService,
				null,
				parametersService
			);

			// Создаем VersionCheckerService
			var versionChecker = new VersionCheckerService(
				checkBaseVersion,
				applicationUpdater,
				dbUpdater,
				skipVersionState
			);

			// Act
			var result = versionChecker.RunUpdate();

			// Assert
			Assert.IsTrue(dialogOpened, "Диалог обновления должен был открыться, так как база новее программы");
			Assert.IsNotNull(result);
		}

		/// <summary>
		/// Тест 4: Версия базы 2.10, программы 2.8.34, установлен стабильный канал.
		/// При запросе в стабильный канал обновлений нет, но так как база новее - предлагается переключить канал.
		/// После переключения на текущий канал появляется обновление 2.10 и должен открыться диалог.
		/// </summary>
		[Test(Description = "Проверяем переключение канала обновлений когда база новее и в текущем канале нет обновлений")]
		public void RunUpdate_WhenDatabaseNewerAndNeedChannelSwitch_ShouldOpenUpdateDialog()
		{
			// Arrange
			var appVersion = new Version(2, 8, 34);
			var updateVersion = new Version(2, 10);
			var databaseVersion = "2.10";

			// Мокаем ParametersService для имитации версии базы 2.10
			var parametersService = CreateParametersService("TestProduct", databaseVersion, "standard");

			// Создаем ApplicationInfo с версией программы 2.8.34
			var applicationInfo = CreateApplicationInfo("TestProduct", appVersion, new[] { "standard" });

			// Создаем CheckBaseVersion
			var checkBaseVersion = new CheckBaseVersion(applicationInfo, parametersService);

			// Мокаем зависимости
			var quitService = Substitute.For<IApplicationQuitService>();
			var dbUpdater = Substitute.For<IDBUpdater>();
			dbUpdater.HasUpdates.Returns(false);

			// Настраиваем конфигурацию без пропуска версии
			var configuration = Substitute.For<IChangeableConfiguration>();
			configuration["AppUpdater:SkipVersion"].Returns((string)null);

			var skipVersionState = new SkipVersionStateIniConfig(configuration);

			// Создаем мок для UpdateChannelService
			var channelService = Substitute.For<IUpdateChannelService>();
			channelService.CurrentChannel.Returns(UpdateChannel.Stable); // Изначально стабильный канал
			channelService.AvailableChannels.Returns(new[] { UpdateChannel.Stable, UpdateChannel.Current });

			// Создаем реальный ApplicationUpdater
			bool dialogOpened = false;
			
			// Создаем ReleasesService с двумя разными ответами для разных каналов
			var releasesService = CreateReleasesServiceWithChannels(updateVersion.ToString());
			
			var navigationManager = CreateNavigationManager(() => dialogOpened = true, CloseSource.Self);
			var interactiveService = Substitute.For<IInteractiveService>();
			
			// Настраиваем ответ на вопрос о переключении канала - пользователь выбирает "Current"
			interactiveService.Question(
				Arg.Is<string[]>(args => args.Contains("Текущий")), 
				Arg.Any<string>()
			).Returns("Текущий");
			
			var guiDispatcher = new GuiDispatcherForTests();

			var applicationUpdater = new ApplicationUpdater(
				releasesService,
				applicationInfo,
				navigationManager,
				interactiveService,
				guiDispatcher,
				quitService,
				channelService,
				parametersService
			);

			// Создаем VersionCheckerService
			var versionChecker = new VersionCheckerService(
				checkBaseVersion,
				applicationUpdater,
				dbUpdater,
				skipVersionState
			);

			// Act
			var result = versionChecker.RunUpdate();

			// Assert
			Assert.IsTrue(dialogOpened, "Диалог обновления должен был открыться после переключения канала");
			Assert.IsNotNull(result);
			
			// Проверяем что был вызван вопрос о переключении канала
			interactiveService.Received(1).Question(
				Arg.Is<string[]>(args => args.Contains("Текущий")), 
				Arg.Any<string>()
			);
			
			// Проверяем что ReleasesService был вызван дважды - для Stable и для Current
			releasesService.Received(2).CheckForUpdates(
				Arg.Any<int>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<ReleaseChannel>()
			);
		}

		/// <summary>
		/// Тест 5: Версия базы 2.8, программы 2.8.5, установлен канал OffAutoUpdate.
		/// При запросе с каналом OffAutoUpdate автоматическое обновление должно быть отключено.
		/// Диалог обновления не должен открываться, даже если есть доступные обновления.
		/// </summary>
		[Test(Description = "Проверяем что канал OffAutoUpdate отключает автоматическое обновление")]
		public void RunUpdate_WhenChannelIsOffAutoUpdate_ShouldNotCheckForUpdates()
		{
			// Arrange
			var appVersion = new Version(2, 8, 5);
			var updateVersion = new Version(2, 9, 1);

			// Мокаем ParametersService для имитации версии базы
			var parametersService = CreateParametersService("TestProduct", "2.8", "standard");

			// Создаем ApplicationInfo с версией программы 2.8.5
			var applicationInfo = CreateApplicationInfo("TestProduct", appVersion, new[] { "standard" });

			// Создаем CheckBaseVersion
			var checkBaseVersion = new CheckBaseVersion(applicationInfo, parametersService);

			// Мокаем зависимости
			var quitService = Substitute.For<IApplicationQuitService>();
			var dbUpdater = Substitute.For<IDBUpdater>();
			dbUpdater.HasUpdates.Returns(false);

			// Настраиваем конфигурацию без пропуска версии
			var configuration = Substitute.For<IChangeableConfiguration>();
			configuration["AppUpdater:SkipVersion"].Returns((string)null);

			var skipVersionState = new SkipVersionStateIniConfig(configuration);

			// Создаем мок для UpdateChannelService с каналом OffAutoUpdate
			var channelService = Substitute.For<IUpdateChannelService>();
			channelService.CurrentChannel.Returns(UpdateChannel.Off);
			channelService.AvailableChannels.Returns(new[] { UpdateChannel.Current, UpdateChannel.Stable, UpdateChannel.Off });

			// Создаем реальный ApplicationUpdater
			bool dialogOpened = false;
			var releasesService = CreateReleasesService(updateVersion.ToString());
			var navigationManager = CreateNavigationManager(() => dialogOpened = true, CloseSource.Self);
			var interactiveService = Substitute.For<IInteractiveService>();
			var guiDispatcher = new GuiDispatcherForTests();

			var applicationUpdater = new ApplicationUpdater(
				releasesService,
				applicationInfo,
				navigationManager,
				interactiveService,
				guiDispatcher,
				quitService,
				channelService,
				parametersService
			);

			// Создаем VersionCheckerService
			var versionChecker = new VersionCheckerService(
				checkBaseVersion,
				applicationUpdater,
				dbUpdater,
				skipVersionState
			);

			// Act
			var result = versionChecker.RunUpdate();

			// Assert
			Assert.IsFalse(dialogOpened, "Диалог обновления НЕ должен был открыться, так как установлен канал OffAutoUpdate");
			
			// Проверяем что ReleasesService НЕ был вызван, так как автообновление отключено
			releasesService.DidNotReceive().CheckForUpdates(
				Arg.Any<int>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<ReleaseChannel>()
			);
			
			// Проверяем что база проверена корректно
			Assert.AreEqual(CheckBaseResult.Ok, checkBaseVersion.Result);
		}

		/// <summary>
		/// Тест 6: Версия базы 2.9.1, программы 2.8.35, установлен канал Текущий.
		/// Серийный номер профессиональный, но истёкший (подписка закончилась).
		/// Сервер возвращает пустой список обновлений с информацией об истёкшей подписке.
		/// В этом случае программа НЕ должна предлагать переключение канала (это бессмысленно),
		/// а должна вернуть результат с проблемой версии базы (BaseVersionGreater).
		/// Пользователь получит стандартное сообщение о том, что версия базы новее программы.
		/// </summary>
		[Test(Description = "Проверяем что при истёкшей подписке НЕ предлагается переключение канала, возвращается ошибка версии базы")]
		public void RunUpdate_WhenDatabaseNewerButSubscriptionExpired_ShouldNotOfferChannelSwitch()
		{
			// Arrange
			var appVersion = new Version(2, 8, 35);
			var databaseVersion = "2.9.1";

			// Мокаем ParametersService для имитации версии базы 2.9.1 с профессиональной редакцией
			var parametersService = CreateParametersService("TestProduct", databaseVersion, "professional");

			// Создаем ApplicationInfo с версией программы 2.8.35
			var applicationInfo = CreateApplicationInfo("TestProduct", appVersion, new[] { "professional" });

			// Создаем CheckBaseVersion
			var checkBaseVersion = new CheckBaseVersion(applicationInfo, parametersService);

			// Мокаем зависимости
			var quitService = Substitute.For<IApplicationQuitService>();
			var dbUpdater = Substitute.For<IDBUpdater>();
			dbUpdater.HasUpdates.Returns(false);

			// Настраиваем конфигурацию без пропуска версии
			var configuration = Substitute.For<IChangeableConfiguration>();
			configuration["AppUpdater:SkipVersion"].Returns((string)null);

			var skipVersionState = new SkipVersionStateIniConfig(configuration);

			// Создаем мок для UpdateChannelService с каналом Текущий
			var channelService = Substitute.For<IUpdateChannelService>();
			channelService.CurrentChannel.Returns(UpdateChannel.Current);
			channelService.AvailableChannels.Returns(new[] { UpdateChannel.Current, UpdateChannel.Stable });

			// Создаем ReleasesService который вернет ответ об истёкшей подписке
			// Важно: список релизов пустой, так как подписка истекла
			var releasesService = Substitute.For<ReleasesService>();
			var response = new CheckForUpdatesResponse
			{
				SubscriptionStatus = SubscriptionStatus.Expired,
				Title = "Подписка истекла",
				Message = "Ваша подписка закончилась. Пожалуйста, продлите подписку, чтобы получать обновления."
			};
			// Releases пустой - обновлений нет из-за истёкшей подписки
			
			releasesService.CheckForUpdates(
				Arg.Any<int>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<ReleaseChannel>()
			).Returns(response);

			// Создаем реальный ApplicationUpdater
			bool dialogOpened = false;
			var navigationManager = CreateNavigationManager(() => dialogOpened = true, CloseSource.Self);
			var interactiveService = Substitute.For<IInteractiveService>();
			
			var guiDispatcher = new GuiDispatcherForTests();

			var applicationUpdater = new ApplicationUpdater(
				releasesService,
				applicationInfo,
				navigationManager,
				interactiveService,
				guiDispatcher,
				quitService,
				channelService,
				parametersService
			);

			// Создаем VersionCheckerService
			var versionChecker = new VersionCheckerService(
				checkBaseVersion,
				applicationUpdater,
				dbUpdater,
				skipVersionState
			);

			// Act
			var result = versionChecker.RunUpdate();

			// Assert
			Assert.IsFalse(dialogOpened, "Диалог обновления НЕ должен был открыться при истёкшей подписке");
			
			// Проверяем что НЕ был вызван вопрос о переключении канала (это бессмысленно при истёкшей подписке)
			interactiveService.DidNotReceive().Question(
				Arg.Any<string[]>(), 
				Arg.Any<string>()
			);
			
			// Проверяем что вернулся результат с проблемой версии базы
			Assert.IsNotNull(result);
			Assert.AreEqual(UpdateStatus.BaseError, result.Value.Status);
			Assert.AreEqual(CheckBaseResult.BaseVersionGreater, checkBaseVersion.Result);
		}

		#region Вспомогательные методы

		private ParametersService CreateParametersService(string productName, string version, string edition)
		{
			// Создаем словарь параметров для тестов
			var parameters = new System.Collections.Generic.Dictionary<string, string>
			{
				{ "product_name", productName },
				{ "version", version },
				{ "edition", edition }
			};

			return new ParametersService(parameters);
		}

		private IApplicationInfo CreateApplicationInfo(string productName, Version version, string[] compatibleModifications)
		{
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductName.Returns(productName);
			appInfo.Version.Returns(version);
			appInfo.CompatibleModifications.Returns(compatibleModifications);
			appInfo.ProductCode.Returns((byte)1);
			appInfo.ProductTitle.Returns(productName);
			appInfo.Modification.Returns("standard");
			return appInfo;
		}

		private ReleasesService CreateReleasesService(string version)
		{
			// Создаем мок сервиса, который возвращает обновление
			var service = Substitute.For<ReleasesService>();
			
			var response = new CheckForUpdatesResponse();
			var release = new ReleaseInfo
			{
				Version = version,
				Changes = "Test changes",
				DatabaseUpdate = DatabaseUpdate.None,
				InstallerLink = "http://test.com/installer.exe"
			};
			response.Releases.Add(release);

			service.CheckForUpdates(
				Arg.Any<int>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<ReleaseChannel>()
			).Returns(response);

			return service;
		}

		private ReleasesService CreateReleasesServiceWithChannels(string currentChannelVersion)
		{
			// Создаем мок сервиса с разными ответами для разных каналов
			var service = Substitute.For<ReleasesService>();
			
			service.CheckForUpdates(
				Arg.Any<int>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<string>(),
				Arg.Any<ReleaseChannel>()
			).Returns(callInfo =>
			{
				var channel = (ReleaseChannel)callInfo[4];
				var response = new CheckForUpdatesResponse();
				
				// Для стабильного канала - нет обновлений
				if (channel == ReleaseChannel.Stable)
				{
					return response; // Пустой список релизов
				}
				
				// Для текущего канала - есть обновление
				if (channel == ReleaseChannel.Current)
				{
					var release = new ReleaseInfo
					{
						Version = currentChannelVersion,
						Changes = "Update from current channel",
						DatabaseUpdate = DatabaseUpdate.None,
						InstallerLink = "http://test.com/installer.exe"
					};
					response.Releases.Add(release);
				}
				
				return response;
			});

			return service;
		}

		private INavigationManager CreateNavigationManager(Action onDialogOpened, CloseSource closeSource)
		{
			var navigationManager = Substitute.For<INavigationManager>();
			var page = Substitute.For<IPage<NewVersionViewModel>>();

			navigationManager.OpenViewModel<NewVersionViewModel, ReleaseInfo[]>(
				Arg.Any<DialogViewModelBase>(),
				Arg.Any<ReleaseInfo[]>(),
				Arg.Any<OpenPageOptions>(),
				Arg.Any<Action<NewVersionViewModel>>(),
				Arg.Any<Action<Autofac.ContainerBuilder>>()
			).Returns(callInfo =>
			{
				onDialogOpened?.Invoke();
				// Возвращаем страницу, событие PageClosed будет вызвано позже
				// Используем Task чтобы событие сработало асинхронно после того как подписчик добавлен
				System.Threading.Tasks.Task.Run(() =>
				{
					System.Threading.Thread.Sleep(10); // Даем время подписаться на событие
					page.PageClosed += Raise.EventWith(new PageClosedEventArgs(closeSource));
				});
				return page;
			});

			return navigationManager;
		}

		#endregion
	}
}

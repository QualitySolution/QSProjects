using Microsoft.Extensions.DependencyInjection;
using QS.ErrorReporting;
using QS.Launcher;
using QS.Utilities.Extensions;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace QS.LibsTest.Core.ErrorReporting {
	/// <summary>
	/// Точка входа сборки. Тестовый хост её не зовёт - она нужна, чтобы ту же сборку можно было
	/// запустить отдельным процессом и уронить его: см. <see cref="CrashProbeTest"/>
	/// </summary>
	public static class CrashProbe {
		/// <summary>Признак запуска пробы. Без него сборку запустили как обычно, и делать нечего</summary>
		public const string Argument = "crash-probe";

		/// <summary>Падение при сборке конфигурации лаунчера - до контейнера и до Avalonia</summary>
		public const string StartupScenario = "startup";

		/// <summary>Падение в фоновом потоке: сети потока интерфейса его не видят</summary>
		public const string BackgroundScenario = "background";

		public const string BackgroundCrashMessage = "падение в фоновом потоке";

		/// <summary>Имени такого ресурса в сборке нет - в этом и смысл</summary>
		private const string MissingResource = "QS.LibsTest.Core.Icons.logo.png";

		public static int Main(string[] args) {
			if(args.Length < 4 || args[0] != Argument)
				return 0;

			var reporting = new CrashReporting { Reporter = new MarkerWriter(args[1]) };
			if(args[2] == "subscribe")
				reporting.Subscribe();

			Crash(args[3]);
			return 0;
		}

		private static void Crash(string scenario) {
			if(scenario == StartupScenario) {
				ConfigureLauncher();
				return;
			}

			// обычный фоновый поток: ни ReactiveUI, ни диспетчер Avalonia о нём не знают,
			// а try/catch вокруг Start бесполезен - исключение всплывает уже в чужом потоке
			new Thread(() => throw new InvalidOperationException(BackgroundCrashMessage)).Start();

			Thread.Sleep(TimeSpan.FromMinutes(1)); //столько процесс не проживёт
		}

		/// <summary>
		/// Сборка настроек лаунчера так же, как её делает продукт: логотип и иконка берутся
		/// встроенным ресурсом по имени-строке. Имя не сходится - и GetResourceByteArray
		/// роняет старт, не добравшись даже до контейнера
		/// </summary>
		private static void ConfigureLauncher() {
			var options = new LauncherOptions {
				AppTitle = "проба",
				LogoImage = typeof(CrashProbe).GetTypeInfo().Assembly.GetResourceByteArray(MissingResource)
			};

			new ServiceCollection()
				.AddLauncherOptions(options)
				.AddLauncherDependencies()
				.AddLauncherViewModels()
				.AddLauncherErrorHandling()
				.BuildServiceProvider();
		}

		/// <summary>Отправка отчёта, которую видно снаружи процесса</summary>
		private sealed class MarkerWriter : IErrorReporter {
			private readonly string path;

			public MarkerWriter(string path) {
				this.path = path;
			}

			public bool SendReport(Exception exception, ErrorType type = ErrorType.Automatic) {
				File.WriteAllText(path, $"{type}: {exception?.GetType().Name}: {exception?.Message}");
				return true;
			}
		}
	}
}

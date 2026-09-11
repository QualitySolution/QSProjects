using NUnit.Framework;
using QS.ErrorReporting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace QS.LibsTest.Core.ErrorReporting {
	/// <summary>
	/// Единственный способ проверить <see cref="CrashReporting"/> по-настоящему - уронить процесс.
	/// В процессе тестового хоста этого не сделать, поэтому падение устраивается в дочернем:
	/// та же сборка запускается с аргументом-признаком и падает по указанному сценарию.
	/// </summary>
	[TestFixture(TestOf = typeof(CrashReporting))]
	[NonParallelizable]
	public class CrashProbeTest {
		private static readonly TimeSpan Patience = TimeSpan.FromSeconds(20);

		/// <summary>Столько ждём отсутствия отчёта - вдвое больше, чем уходит на его отправку</summary>
		private static readonly TimeSpan ShortWait = TimeSpan.FromSeconds(5);

		private string markerPath;

		[SetUp]
		public void SetUp() =>
			markerPath = Path.Combine(Path.GetTempPath(), "qs-crash-probe-" + Guid.NewGuid().ToString("N") + ".txt");

		[TearDown]
		public void TearDown() {
			if(File.Exists(markerPath))
				File.Delete(markerPath);
		}

		[Test(Description = "Настоящее падение на старте: логотип берётся ресурсом, имя ресурса не сошлось")]
		public void StartupCrash_Subscribed_ReportSentBeforeProcessDies() {
			bool reported, exited;
			using(var probe = StartProbe(CrashProbe.StartupScenario, subscribe: true)) {
				reported = WaitForMarker(Patience);
				exited = probe.WaitForExit((int)Patience.TotalMilliseconds);
				Kill(probe);
			}

			Assert.That(reported, Is.True,
				"на старте контейнера ещё нет, сети потока интерфейса не подняты - ловить больше нечем");
			Assert.That(File.ReadAllText(markerPath),
				Does.Contain(nameof(NullReferenceException)).And.Contain(ErrorType.Automatic.ToString()),
				"отсутствующий ресурс приходит пустым потоком, а падает уже на копировании; " +
				"отправляется само - спрашивать пользователя уже некому");
			Assert.That(exited, Is.True,
				"процесс всё равно умирает - перехват даёт только успеть отправить отчёт");
		}

		[Test(Description = "Без подписки то же падение не оставляет следов: приложение умирает молча")]
		public void StartupCrash_NotSubscribed_DiesSilently() {
			bool reported;
			using(var probe = StartProbe(CrashProbe.StartupScenario, subscribe: false)) {
				reported = WaitForMarker(ShortWait);
				Kill(probe);
			}

			Assert.That(reported, Is.False,
				"отчёт взяться неоткуда: AppDomain.UnhandledException - единственное событие про это падение");
		}

		[Test(Description = "Падение фонового потока идёт мимо ReactiveUI и диспетчера - тоже только сюда")]
		public void BackgroundThreadCrash_Subscribed_ReportSent() {
			bool reported;
			using(var probe = StartProbe(CrashProbe.BackgroundScenario, subscribe: true)) {
				reported = WaitForMarker(Patience);
				probe.WaitForExit((int)ShortWait.TotalMilliseconds);
				Kill(probe);
			}

			Assert.That(reported, Is.True, "фоновый поток поймать больше некому");
		}

		private Process StartProbe(string scenario, bool subscribe) {
			string assembly = typeof(CrashProbeTest).GetTypeInfo().Assembly.Location;
			var start = new ProcessStartInfo("dotnet") {
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			foreach(var argument in new[] {
				"exec", assembly, CrashProbe.Argument, markerPath,
				subscribe ? "subscribe" : "no-subscribe", scenario })
				start.ArgumentList.Add(argument);

			var process = Process.Start(start);
			Assert.That(process, Is.Not.Null, "не удалось запустить дочерний процесс пробы");
			return process;
		}

		private bool WaitForMarker(TimeSpan timeout) {
			var elapsed = Stopwatch.StartNew();
			while(elapsed.Elapsed < timeout) {
				if(File.Exists(markerPath))
					return true;
				Thread.Sleep(50);
			}
			return File.Exists(markerPath);
		}

		/// <summary>
		/// Труп процесса на Windows может задержать отчёт системы об ошибке - тест этого не ждёт
		/// </summary>
		private static void Kill(Process process) {
			try {
				if(!process.HasExited)
					process.Kill();
			}
			catch(InvalidOperationException) { } //успел выйти сам между проверкой и Kill
		}
	}
}

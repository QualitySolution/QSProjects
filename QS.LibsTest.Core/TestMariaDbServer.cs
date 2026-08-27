using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Testcontainers.MariaDb;

/// <summary>
/// Один сервер MariaDB на весь прогон сборки.
///
/// Контейнер в <see cref="QS.Testing.DB.MariaDbTestContainerSqlFixtureBase"/> - поле экземпляра,
/// то есть по серверу на каждую фикстуру. Для десяти фикстур лаунчера это десять MariaDB разом,
/// и виртуальная машина Docker кончается раньше тестов: контейнеры не стартуют либо их убивает
/// по памяти посреди теста. Выглядит это как «тесты влияют друг на друга», хотя данными они
/// не пересекаются.
///
/// Выигрыша от параллельного запуска тут всё равно нет: старт сервера - десятки секунд,
/// сами тесты фикстуры - секунда. Поэтому сервер общий, а фикстуры идут по очереди.
///
/// Класс намеренно без namespace: только так NUnit считает SetUpFixture относящимся
/// ко всей сборке, а не к одному пространству имён.
/// </summary>
[SetUpFixture]
public class TestMariaDbServer {
	/// <summary>Версия закреплена: иначе она уезжает вместе с версией пакета Testcontainers</summary>
	private const string Image = "mariadb:10.11";

	private static readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
	private static MariaDbContainer container;

	/// <summary>
	/// Сервер поднимается при первом обращении, а не в OneTimeSetUp: прогон тестов,
	/// которым база не нужна, не должен ждать Docker
	/// </summary>
	public static async Task<MariaDbContainer> GetAsync() {
		await gate.WaitAsync();
		try {
			if(container == null)
				container = await StartAsync();
			return container;
		}
		finally {
			gate.Release();
		}
	}

	private static async Task<MariaDbContainer> StartAsync() {
		var started = new MariaDbBuilder(Image)
			.WithUsername("root")
			.WithPassword("root")
			.WithCommand("--character-set-server=utf8mb4"
				, "--collation-server=utf8mb4_general_ci"
				, "--skip-name-resolve"
				// сервер живёт один на весь прогон и данных в нём мало - буфер по умолчанию избыточен
				, "--innodb-buffer-pool-size=64M"
				// провайдер держит свой пул на каждый логин, а тесты входят десятками учёток;
				// со 151 по умолчанию общий прогон упирается в «Too many connections»
				, "--max-connections=500")
			.Build();

		await started.StartAsync();
		return started;
	}

	[OneTimeTearDown]
	public async Task StopServer() {
		if(container == null)
			return;

		await container.DisposeAsync();
		container = null;
	}
}

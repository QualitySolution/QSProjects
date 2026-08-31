using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Testcontainers.MariaDb;

[SetUpFixture]
public class TestMariaDbServer {
	private const string Image = "mariadb:10.11";

	private static readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
	private static MariaDbContainer container;

	//прогон тестов, которым база не нужна, не должен ждать Docker
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
				, "--innodb-buffer-pool-size=64M"
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

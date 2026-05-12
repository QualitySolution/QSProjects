using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace QS.ErrorReporting
{
	/// <summary>
	/// Читает логи текущего Docker-контейнера через Docker socket (/var/run/docker.sock).
	/// Для работы необходимо пробросить сокет в контейнер:
	///   volumes:
	///     - /var/run/docker.sock:/var/run/docker.sock:ro
	/// Если приложение запускается не от root, контейнерный пользователь должен состоять
	/// в группе, которой принадлежит docker.sock на хосте. Для переносимого деплоя
	/// вычисляйте GID на целевом сервере и передавайте его в compose через .env:
	///   DOCKER_SOCK_GID=$(stat -c %g /var/run/docker.sock)
	///   group_add:
	///     - "${DOCKER_SOCK_GID}"
	/// </summary>
	public class DockerLogService : IAsyncLogService
	{
		// В Docker hostname контейнера == короткий ID контейнера
		private static readonly string ContainerId = System.Net.Dns.GetHostName();

		public async Task<string> GetLogAsync(uint? rowCount = null)
		{
			var tail = rowCount?.ToString() ?? "300";

			using (var client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
				.CreateClient())
			{
				var parameters = new ContainerLogsParameters
				{
					ShowStdout = true,
					ShowStderr = true,
					Tail = tail,
					Timestamps = false,
				};

				using (var stream = await client.Containers.GetContainerLogsAsync(
					ContainerId, false, parameters, CancellationToken.None))
				{
					var result = await stream.ReadOutputToEndAsync(CancellationToken.None);
					return result.stdout + result.stderr;
				}
			}
		}
	}
}

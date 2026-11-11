using System;
using System.Threading;
using System.Threading.Tasks;
using QS.Configuration;
using QS.Project.Versioning;
using QS.Updates;
using QS.Utilities.Text;

namespace QS.Updater.App {
	/// <summary>Проверка новых версий без открытия диалогов и подключения к БД.</summary>
	public sealed class ApplicationUpdateChecker {
		private readonly ReleasesService releases;
		private readonly IApplicationInfo application;
		private readonly IUpdateChannelService channels;
		private readonly IChangeableConfiguration configuration;

		public ApplicationUpdateChecker(ReleasesService releases, IApplicationInfo application,
			IUpdateChannelService channels, IChangeableConfiguration configuration) {
			this.releases = releases;
			this.application = application;
			this.channels = channels;
			this.configuration = configuration;
		}

		public async Task<CheckForUpdatesResponse> CheckAsync(CancellationToken cancellationToken, string serial = "") {
			configuration.Reload();
			if(channels.CurrentChannel == UpdateChannel.Off)
				return new CheckForUpdatesResponse();
			var response = await releases.CheckForUpdatesAsync(application.ProductCode, application.Version.VersionToShortString(),
				application.Modification ?? string.Empty, serial,
				channels.CurrentChannel == UpdateChannel.Stable ? ReleaseChannel.Stable : ReleaseChannel.Current, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			return response;
		}
	}
}

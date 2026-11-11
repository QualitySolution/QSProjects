using System;
using System.Collections.Generic;
using QS.Configuration;

namespace QS.Updater.App {
	public sealed class UpdateChannelService : IUpdateChannelService {
		private readonly IChangeableConfiguration configuration;
		public UpdateChannelService(IChangeableConfiguration configuration) { this.configuration = configuration; }
		public UpdateChannel CurrentChannel => Enum.TryParse(configuration["AppUpdater:Channel"], true, out UpdateChannel channel)
			&& Enum.IsDefined(typeof(UpdateChannel), channel) ? channel : UpdateChannel.Current;
		public IEnumerable<UpdateChannel> AvailableChannels => (UpdateChannel[])Enum.GetValues(typeof(UpdateChannel));
	}
}

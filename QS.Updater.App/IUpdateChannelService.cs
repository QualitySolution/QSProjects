using System.Collections.Generic;

namespace QS.Updater.App {
	public interface IUpdateChannelService {
		UpdateChannel CurrentChannel { get; }
		
		IEnumerable<UpdateChannel> AvailableChannels { get; }
	}
}

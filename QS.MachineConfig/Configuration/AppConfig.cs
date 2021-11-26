using System.Collections.Generic;

namespace QS.MachineConfig.Configuration
{
	class AppConfig : IAppConfig
	{
		public IList<Connection> Connections { get; set; }
	}
}

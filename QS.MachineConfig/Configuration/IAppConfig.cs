using System.Collections.Generic;

namespace QS.MachineConfig.Configuration
{
	public interface IAppConfig
	{
		IList<Connection> Connections { get; set; }
	}
}

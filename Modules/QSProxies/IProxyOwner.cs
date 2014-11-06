using System;
using System.Collections.Generic;

namespace QSProxies
{
	public interface IProxyOwner
	{
		IList<Proxy> Proxies { get; set;}
	}
}


using System;
using QSOrmProject;
using System.Collections.Generic;

namespace QSProxies
{
	public static class QSProxiesMain
	{

		public static List<OrmObjectMaping> GetModuleMaping()
		{
			return new List<OrmObjectMaping>
			{
				new OrmObjectMaping(typeof(Proxy), typeof(ProxyDlg), "{QSProxies.Proxy} Number[Номер]; StartDate[С]; ExpirationDate[По]")
			};
		}
	}
}

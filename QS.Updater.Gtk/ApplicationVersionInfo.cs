using System;
using QSSupportLib;

namespace QS.Updater
{
	public struct ApplicationVersionInfo : IApplicationInfo
	{
		public string ProductName => MainSupport.ProjectVerion.Product;

		public string Edition => MainSupport.ProjectVerion.Edition;

		public Version Version => MainSupport.ProjectVerion.Version;

		public string SerialNumber => MainSupport.BaseParameters.SerialNumber;
	}
}

using System;
using QS.Project.VersionControl;
using QSSupportLib;

namespace QS.Updater
{
	//TODO Временный набросок интерфейса, служит пока для разрыва связи с QSSupport, нужно будет переделать расширить и дополнить.
	public struct ApplicationVersionInfo : IApplicationInfo
	{
		public string ProductName => MainSupport.ProjectVerion.Product;

		public string Edition => MainSupport.ProjectVerion.Edition;

		public Version Version => MainSupport.ProjectVerion.Version;

		public string SerialNumber => MainSupport.BaseParameters.SerialNumber;

		public string DBName => String.Empty;
	}
}

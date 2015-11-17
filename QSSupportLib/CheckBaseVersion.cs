using System;
using QSProjectsLib;

namespace QSSupportLib
{
	public static class CheckBaseVersion
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static CheckBaseResult ResultFlags { get; private set;}

		public static string TextMessage { get; private set;}

		/// <summary>
		/// Проверяем версию базы. Можно запускать неоднократно, вернет true если есть сообщение.
		/// </summary>
		public static bool Check ()
		{
			ResultFlags = CheckBaseResult.Ok;

			if (string.IsNullOrWhiteSpace (MainSupport.BaseParameters.Product)) {
				ResultFlags |= CheckBaseResult.IncorrectProduct;
				TextMessage = "Название продукта в базе данных не указано.";
				return true;
			}

			if (MainSupport.BaseParameters.Product != MainSupport.ProjectVerion.Product) {
				ResultFlags |= CheckBaseResult.IncorrectProduct;
				TextMessage = "База данных от другого продукта продукта.";
				logger.Fatal ("База данных от другого продукта продукта. (База: {0} Программа: {1})",
					MainSupport.BaseParameters.Product,
					MainSupport.ProjectVerion.Product
				);
				return true;
			}

			Version baseVersion;

			if (String.IsNullOrWhiteSpace (MainSupport.BaseParameters.Version) 
				|| !Version.TryParse (MainSupport.BaseParameters.Version, out baseVersion)) {
				ResultFlags |= CheckBaseResult.IncorrectVersion;
				TextMessage = "Версия базы данных не определена.";
				return true;
			}
				
			if (MainSupport.ProjectVerion.Version.Major != baseVersion.Major || MainSupport.ProjectVerion.Version.Minor != baseVersion.Minor) {
				TextMessage = "Версия продукта не совпадает с версией базы данных.\n";
				TextMessage += String.Format ("Версия продукта: {0}", StringWorks.VersionToShortString (MainSupport.ProjectVerion.Version)); 
				TextMessage += "\nВерсия базы данных: " + MainSupport.BaseParameters.Version;

				if(MainSupport.ProjectVerion.Version > baseVersion)
					ResultFlags |= CheckBaseResult.BaseVersionLess;
				else
					ResultFlags |= CheckBaseResult.BaseVersionGreater;
				return true;
			}

			if (String.IsNullOrWhiteSpace (MainSupport.BaseParameters.Edition) ) {
				TextMessage = "Редакция базы не указана!";
				ResultFlags |= CheckBaseResult.UnsupportEdition;
				return true;
			}

			if (!MainSupport.ProjectVerion.AllowEdition.Contains (MainSupport.BaseParameters.Edition)) {
				ResultFlags |= CheckBaseResult.UnsupportEdition;
				TextMessage = "Редакция базы данных не поддерживается.\n";
				TextMessage += "Редакция продукта: " + MainSupport.ProjectVerion.Edition + "\nРедакция базы данных: " + MainSupport.BaseParameters.Edition;
				return true;
			}

			return false;
		}
	}

	[Flags]
	public enum CheckBaseResult
	{
		Ok = 0x0,
		IncorrectProduct = 0x1,
		UnsupportEdition = 0x2,
		IncorrectVersion = 0x4,
		BaseVersionLess = 0x8,
		BaseVersionGreater = 0x16
	}
}


using System;
using System.Linq;
using QS.BaseParameters;

namespace QS.Project.Versioning
{
	public class CheckBaseVersion
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Результат
		public CheckBaseResult ResultFlags { get; private set;}

		public string TextMessage { get; private set;}
		#endregion

		private readonly IApplicationInfo ApplicationInfo;
		private readonly dynamic parametersService;

		public CheckBaseVersion(IApplicationInfo applicationInfo, ParametersService parametersService)
		{
			ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.parametersService = parametersService ?? throw new ArgumentNullException(nameof(parametersService));
		}

		/// <summary>
		/// Проверяем версию базы. Можно запускать неоднократно, вернет true если есть сообщение.
		/// </summary>
		public bool Check ()
		{
			ResultFlags = CheckBaseResult.Ok;

			if (string.IsNullOrWhiteSpace (parametersService.product_name)) {
				ResultFlags |= CheckBaseResult.IncorrectProduct;
				TextMessage = "Название продукта в базе данных не указано.";
				return true;
			}

			if (parametersService.product_name != ApplicationInfo.ProductName) {
				ResultFlags |= CheckBaseResult.IncorrectProduct;
				TextMessage = "База данных от другого продукта.";
				logger.Fatal ("База данных от другого продукта. (База: {0} Программа: {1})",
					parametersService.product_name,
					ApplicationInfo.ProductName
				);
				return true;
			}

			if(ApplicationInfo.CompatibleModifications.Length > 0) {
				if(String.IsNullOrWhiteSpace(parametersService.edition)) {
					TextMessage = "Редакция базы не указана!";
					ResultFlags |= CheckBaseResult.UnsupportedEdition;
					return true;
				}

				if(!ApplicationInfo.CompatibleModifications.Contains((string)parametersService.edition)) {
					ResultFlags |= CheckBaseResult.UnsupportedEdition;
					TextMessage = "Редакция базы данных не поддерживается.\n";
					TextMessage += "Поддерживаемые редакции: " + String.Join(" ,", ApplicationInfo.CompatibleModifications) 
						+ "\nРедакция базы данных: " + parametersService.edition;
					return true;
				}
			}
			Version baseVersion = null;

			if (String.IsNullOrWhiteSpace (parametersService.version) 
				|| !Version.TryParse (parametersService.version, out baseVersion)) {
				ResultFlags |= CheckBaseResult.IncorrectVersion;
				TextMessage = "Версия базы данных не определена.";
				return true;
			}
				
			if (ApplicationInfo.Version.Major != baseVersion.Major || ApplicationInfo.Version.Minor != baseVersion.Minor) {
				TextMessage = "Версия продукта не совпадает с версией базы данных.\n";
				TextMessage += String.Format ("Версия продукта: {0}.{1}", ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor); 
				TextMessage += "\nВерсия базы данных: " + parametersService.version;

				if(ApplicationInfo.Version > baseVersion)
					ResultFlags |= CheckBaseResult.BaseVersionLess;
				else
					ResultFlags |= CheckBaseResult.BaseVersionGreater;
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
		UnsupportedEdition = 0x2,
		IncorrectVersion = 0x4,
		BaseVersionLess = 0x8,
		BaseVersionGreater = 0x16
	}
}


using System;
using System.Linq;
using QS.BaseParameters;

namespace QS.Project.Versioning
{
	public class CheckBaseVersion
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Результат
		public CheckBaseResult Result { get; private set;}

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
			Result = CheckBaseResult.Ok;

			if (string.IsNullOrWhiteSpace (parametersService.product_name)) {
				Result = CheckBaseResult.IncorrectProduct;
				TextMessage = "Название продукта в базе данных не указано.";
				return true;
			}

			if (parametersService.product_name != ApplicationInfo.ProductName) {
				Result = CheckBaseResult.IncorrectProduct;
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
					Result = CheckBaseResult.UnsupportedEdition;
					return true;
				}

				if(!ApplicationInfo.CompatibleModifications.Contains((string)parametersService.edition)) {
					Result = CheckBaseResult.UnsupportedEdition;
					TextMessage = "Редакция базы данных не поддерживается.\n";
					TextMessage += "Поддерживаемые редакции: " + String.Join(" ,", ApplicationInfo.CompatibleModifications) 
						+ "\nРедакция базы данных: " + parametersService.edition;
					return true;
				}
			}
			Version baseVersion = null;

			if (String.IsNullOrWhiteSpace (parametersService.version) 
				|| !Version.TryParse (parametersService.version, out baseVersion)) {
				Result = CheckBaseResult.IncorrectVersion;
				TextMessage = "Версия базы данных не определена.";
				return true;
			}
				
			if (ApplicationInfo.Version.Major != baseVersion.Major || ApplicationInfo.Version.Minor != baseVersion.Minor) {
				TextMessage = "Версия продукта не совпадает с версией базы данных.\n";
				TextMessage += $"Версия продукта: {ApplicationInfo.Version.Major}.{ApplicationInfo.Version.Minor}"; 
				TextMessage += "\nВерсия базы данных: " + parametersService.version;

				if(ApplicationInfo.Version > baseVersion)
					Result = CheckBaseResult.BaseVersionLess;
				else
					Result = CheckBaseResult.BaseVersionGreater;
				return true;
			}

			return false;
		}
	}

	public enum CheckBaseResult
	{
		/// <summary>
		/// Версия базы корректна и позволяет работать.
		/// </summary>
		Ok,
		/// <summary>
		/// База данных от другого продукта.
		/// </summary>
		IncorrectProduct,
		/// <summary>
		/// Не поддерживаемая редакция базы.
		/// </summary>
		UnsupportedEdition,
		/// <summary>
		/// Версия базы не определена.
		/// </summary>
		IncorrectVersion,
		/// <summary>
		/// Программа новее базы. Для продолжения работы обновление базы обязательно.
		/// </summary>
		BaseVersionLess,
		/// <summary>
		/// База данных новее программы. Для продолжения работы необходимо обновление программы.
		/// </summary>
		BaseVersionGreater
	}
}

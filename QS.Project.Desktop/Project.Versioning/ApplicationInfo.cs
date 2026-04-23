using System;
using System.Reflection;

namespace QS.Project.Versioning
{
	/// <summary>
	/// Класс для получения информации о приложении.
	/// Вся информация задается через DI в конструкторе, использовать в приложениях на .NET Core.
	/// </summary>
	public class ApplicationInfo : IApplicationInfo
	{
		private string productName;
		/// <summary>
		/// По умолчанию возвращает имя входной сборки. Можно переопределить при регистрации в DI,
		/// например для того, чтобы лаунчер сопоставлял базы по значению base_parameters.product_name.
		/// </summary>
		public string ProductName {
			get => productName ?? Assembly.GetName().Name;
			set => productName = value;
		}
		public string ProductTitle => Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;

		public string Modification { get; set; }
		public string ModificationTitle { get; set; }
		public bool ModificationIsHidden { get; set; }
		public string[] CompatibleModifications { get; set; } = Array.Empty<string>();

		public Version Version => Assembly.GetName().Version;

		public bool IsBeta { get; set; }

		public DateTime? BuildDate => System.IO.File.GetLastWriteTime(Assembly.Location);

		public byte ProductCode { get; set; }

		#region Внутрение
		private Assembly Assembly => Assembly.GetEntryAssembly();
		#endregion
	}
}

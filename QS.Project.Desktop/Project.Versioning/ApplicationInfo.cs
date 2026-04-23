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
		public string ProductName => Assembly.GetName().Name;
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

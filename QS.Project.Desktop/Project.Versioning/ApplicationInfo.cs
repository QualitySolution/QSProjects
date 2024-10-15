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
		public string[] CompatibleModifications {
			get {
				throw new NotImplementedException();
				// var modificationAttributes = Assembly.GetCustomAttributes<AssemblyCompatibleModificationAttribute>();
				// var list = modificationAttributes.Select(x => x.Name).ToList();
				// if (!String.IsNullOrWhiteSpace(Modification) && !list.Contains(Modification))
				// 	list.Add(Modification);
				// return list.ToArray();
				}
			}

		public Version Version => Assembly.GetName().Version;

		public bool IsBeta { get; set; }

		public DateTime? BuildDate => System.IO.File.GetLastWriteTime(Assembly.Location);

		public byte ProductCode { get; set; }

		#region Внутрение
		private Assembly Assembly => Assembly.GetEntryAssembly();
		#endregion
	}
}

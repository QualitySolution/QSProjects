using System;
using System.Linq;
using System.Reflection;

namespace QS.Project.Versioning
{
	public class ApplicationVersionInfo : IApplicationInfo
	{
		public string ProductName => Assembly.GetName().Name;
		public string ProductTitle => Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;

		public string Modification => Assembly.GetCustomAttribute<AssemblyModificationAttribute>()?.Name;
		public string ModificationTitle => Assembly.GetCustomAttribute<AssemblyModificationAttribute>()?.Title;
		public bool ModificationIsHidden => Assembly.GetCustomAttribute<AssemblyModificationAttribute>()?.HideFromUser ?? true;
		public string[] СompatibleModifications {
			get {
				var modificationAttributes = Assembly.GetCustomAttributes<AssemblyСompatibleModificationAttribute>();
				var list = modificationAttributes.Select(x => x.Name).ToList();
				if (!String.IsNullOrWhiteSpace(Modification) && !list.Contains(Modification))
					list.Add(Modification);
				return list.ToArray();
				}
			}

		public Version Version => Assembly.GetName().Version;

		public bool IsBeta => Assembly.GetCustomAttribute<AssemblyBetaBuildAttribute>() != null;

		public DateTime BuildDate => System.IO.File.GetLastWriteTime(Assembly.Location);

		public byte ProductCode => Assembly.GetCustomAttribute<AssemblyProductCodeAttribute>()?.Number ?? 0;

		#region Внутрение
		private Assembly Assembly => Assembly.GetEntryAssembly();
		#endregion
	}
}

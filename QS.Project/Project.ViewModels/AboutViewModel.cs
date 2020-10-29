using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QS.Project.Versioning;
using QS.Utilities.Text;
using QS.ViewModels;

namespace QS.Project.ViewModels
{
	public class AboutViewModel : ViewModelBase
	{
		public AboutViewModel(IApplicationInfo applicationInfo)
		{
			ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
		}

		public IApplicationInfo ApplicationInfo { get; }

		#region Инфа для View

		public string ProgramName => ApplicationInfo.ProductTitle
			+ (!String.IsNullOrEmpty(ApplicationInfo.ModificationTitle) ? $" {ApplicationInfo.ModificationTitle}" : String.Empty);

		public string Version {
			get {
				var text = ApplicationInfo.Version.VersionToShortString();
				if (String.IsNullOrEmpty(ApplicationInfo.ModificationTitle) && !String.IsNullOrWhiteSpace(ApplicationInfo.Modification))
					text += $"-{ApplicationInfo.Modification}";
				return text;
			}
		}

		public string Description {
			get {
				var att = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

				string comments = String.Empty;

				if (ApplicationInfo.IsBeta) {
					comments += String.Format("Бета редакция от {0:g}\n", ApplicationInfo.BuildDate);
				}

				comments += ((AssemblyDescriptionAttribute)att[0]).Description;

				var support = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblySupportAttribute>()?.SupportInfo;
				comments += support ?? "\nТелефон тех. поддержки +7(812)309-80-89";
				return comments; 
			}
		}

		public string Copyright => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

		public string[] Authors {
			get {
				var authorAttributes = Assembly.GetEntryAssembly().GetCustomAttributes<AssemblyAuthorAttribute>();
				return authorAttributes.Select(x => String.Join(" ", x.Name, x.Email, x.YearsOfActivity)).Reverse().ToArray();
			}
		}

		public string WebLink => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyAppWebsiteAttribute>()?.Link;

		#endregion
	}
}

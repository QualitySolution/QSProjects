using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QS.Project.Versioning;
using QS.Project.Versioning.Product;
using QS.Utilities.Text;
using QS.ViewModels;

namespace QS.Project.ViewModels
{
	public class AboutViewModel : ViewModelBase
	{
		private readonly IProductService productService;

		public AboutViewModel(IApplicationInfo applicationInfo, IProductService productService = null)
		{
			ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.productService = productService;
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
				var description = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
				var support = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblySupportAttribute>()?.SupportInfo;

				var text = new List<string>();

				if(productService?.EditionName != null)
					text.Add(productService?.EditionName);

				if (ApplicationInfo.IsBeta)
					text.Add(String.Format("Бета от {0:g}", ApplicationInfo.BuildDate));

				if(String.IsNullOrWhiteSpace(description))
					text.Add(description);

				if(String.IsNullOrWhiteSpace(support))
					text.Add(support);

				return String.Join("\n", text);
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

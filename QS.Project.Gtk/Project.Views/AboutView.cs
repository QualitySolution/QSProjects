using System;
using System.Reflection;
using Gtk;
using QS.Project.VersionControl;
using QS.Project.ViewModels;

namespace QS.Project.Views
{
	public class AboutView : AboutDialog
	{
		private readonly AboutViewModel viewModel;

		public AboutView(AboutViewModel viewModel)
		{
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

			ProgramName = viewModel.ProgramName;
			Version = viewModel.Version;

			var att = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyLogoIconAttribute), false);
			if (att.Length > 0) {
				Logo = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), ((AssemblyLogoIconAttribute)att[0]).ResourceName);
			}

			Comments = viewModel.Description;
			Copyright = viewModel.Copyright;
			Authors = viewModel.Authors;
			Website = viewModel.WebLink;
		}
	}
}

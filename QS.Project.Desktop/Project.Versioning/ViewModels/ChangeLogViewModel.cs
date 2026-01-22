using System;
using System.IO;
using NLog;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Project.Versioning.ViewModels {
	public class ChangeLogViewModel: DialogViewModelBase {
		
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		
		public ChangeLogViewModel(INavigationManager navigation) : base(navigation)
		{
			Title = "История версий программы";

			using (StreamReader historyFile = new StreamReader ("changes.txt")) {
				TextLog = historyFile.ReadToEnd ();
			}
		}
		
		public string TextLog { get; }
	}
}

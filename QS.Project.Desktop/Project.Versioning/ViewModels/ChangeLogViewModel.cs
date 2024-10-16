using System;
using System.IO;
using NLog;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Project.Versioning.ViewModels {
	public class ChangeLogViewModel: WindowDialogViewModelBase {
		
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		
		public ChangeLogViewModel(INavigationManager navigation) : base(navigation)
		{
			Title = "История версий программы";
			
			try {
				using (StreamReader historyFile = new StreamReader ("changes.txt")) {
					TextLog = historyFile.ReadToEnd ();
				}
			}
			catch (Exception e) {
				logger.Warn("Не удалось открыть changes.txt");
				TextLog = "Не удалось открыть changes.txt";
			}
		}
		
		public string TextLog { get; }
	}
}

using QS.Launcher.ViewModels.TypeViewModels;
using System.Collections.Generic;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class BaseManagementVM : CarouselPageVM {

		public List<DatabaseViewModel> Databases { get; set; }

		public DatabaseViewModel? SelectedDatabase { get; set; }

		public BaseManagementVM() {
			Databases = new() { // sample
				new() { Name = "Kukaracha.db", Size = "39.6 MB" },
				new() { Name = "Simon", Size = "501.0 MB" },
				new() { Name = "OnlyAdmins", Size = "1.0 MB" }
			};
		}
	}
}

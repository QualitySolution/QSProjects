using QS.Launcher.ViewModels.TypeViewModels;
using System.Collections.Generic;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class BaseManagementVM : CarouselPageVM {

		public List<DatabaseViewModel> Databases { get; set; }

		public DatabaseViewModel SelectedDatabase { get; set; }

		public BaseManagementVM() {
			
		}
	}
}

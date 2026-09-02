using Avalonia.Controls;
using QS.Launcher.ViewModels.PageViewModels.DataBase;

namespace QS.Launcher.Views.Pages.DataBase;

public partial class CreateDataBaseSettingsView : UserControl {
	public CreateDataBaseSettingsView(CreateDataBaseSettingsVM settingsVM) {
		InitializeComponent();

		DataContext = settingsVM;
	}
}

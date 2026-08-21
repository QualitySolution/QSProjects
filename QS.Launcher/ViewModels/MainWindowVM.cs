using System;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using QS.ViewModels;

namespace QS.Launcher.ViewModels {
	/// <summary>
	/// Оболочка окна: знает корневые страницы и умеет сохранить подключения при закрытии.
	/// Сам стек страниц ведёт <see cref="LauncherNavigation"/>
	/// </summary>
	public class MainWindowVM : ViewModelBase {
		private readonly LoginVM login;

		public LauncherNavigation Navigation { get; }

		public MainWindowVM(
			LauncherNavigation navigation,
			DataBasesVM dataBasesVM,
			LoginVM loginVM)
		{
			Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			login = loginVM ?? throw new ArgumentNullException(nameof(loginVM));

			// корни ставим здесь, а не конструктором навигатора: иначе страницы, которые
			// сами зависят от навигатора, замкнули бы граф зависимостей
			Navigation.SetRoots(loginVM, dataBasesVM);
		}

		public void SaveConnections() {
			login.SaveConnections();
		}
	}
}

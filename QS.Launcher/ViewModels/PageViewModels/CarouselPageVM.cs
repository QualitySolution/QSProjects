using System.Windows.Input;
using QS.ViewModels;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class CarouselPageVM : ViewModelBase {
		private ICommand nextPageCommand;
		public ICommand NextPageCommand {
			get => nextPageCommand;
			set => this.RaiseAndSetIfChanged(ref nextPageCommand, value);
		}

		private ICommand previousPageCommand;
		public ICommand PreviousPageCommand {
			get => previousPageCommand;
			set => this.RaiseAndSetIfChanged(ref previousPageCommand, value);
		}

		private ICommand changePageCommand;
		public ICommand ChangePageCommand {
			get => changePageCommand;
			set => this.RaiseAndSetIfChanged(ref changePageCommand, value);
		}

		private ICommand pushPageCommand;
		/// <summary>
		/// добавить страницу в конец Carousel и переключить фокус на неё
		/// </summary>
		public ICommand PushPageCommand {
			get => pushPageCommand;
			set => this.RaiseAndSetIfChanged(ref pushPageCommand, value);
		}

		private ICommand popPageCommand;
		/// <summary>
		/// Закрыть текущую нерутовую страницу и вернуться на предыдущую
		/// </summary>
		public ICommand PopPageCommand {
			get => popPageCommand;
			set => this.RaiseAndSetIfChanged(ref popPageCommand, value);
		}

		private ICommand popToRootCommand;
		/// <summary>
		/// Закрыть все нерутовые страницы и вернуться к корневым вкладкам
		/// </summary>
		public ICommand PopToRootCommand {
			get => popToRootCommand;
			set => this.RaiseAndSetIfChanged(ref popToRootCommand, value);
		}

		private ICommand popToPageCommand;
		/// <summary>
		/// Найти первую страницу указанного типа в стеке и переключиться на неё, сняв всё, что стоит выше
		/// </summary>
		public ICommand PopToPageCommand {
			get => popToPageCommand;
			set => this.RaiseAndSetIfChanged(ref popToPageCommand, value);
		}
	}
}

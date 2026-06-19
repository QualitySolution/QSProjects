using System.Windows.Input;
using QS.ViewModels;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class CarouselPageVM : ViewModelBase {
		public ICommand NextPageCommand { get; set; }

		public ICommand PreviousPageCommand { get; set; }

		public ICommand ChangePageCommand { get; set; }

		/// <summary>
		/// добавить страницу в конец Carousel и переключить фокус на неё
		/// </summary>
		public ICommand PushPageCommand { get; set; }

		/// <summary>
		/// Закрыть текущую нерутовую страницу и вернуться на предыдущую
		/// </summary>
		public ICommand PopPageCommand { get; set; }

		/// <summary>
		/// Закрыть все нерутовые страницы и вернуться к корневым вкладкам
		/// </summary>
		public ICommand PopToRootCommand { get; set; }

		/// <summary>
		/// Найти первую страницу указанного типа в стеке и переключиться на неё, сняв всё, что стоит выше
		/// </summary>
		public ICommand PopToPageCommand { get; set; }
	}
}

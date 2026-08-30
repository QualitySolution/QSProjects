using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using QS.ViewModels;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class CarouselPageVM : ViewModelBase {
		/// <summary>
		/// Стек страниц. Приходит конструктором, а не проставляется снаружи: пока навигация
		/// была набором свойств-команд, незаполненная команда молча ничего не делала -
		/// «нажал, и ничего не произошло». Обязательная зависимость превращает это
		/// в ошибку сборки контейнера, то есть в ошибку разработчика, а не в тихое
		/// поведение у пользователя
		/// </summary>
		protected LauncherNavigation Navigation { get; }

		protected CarouselPageVM(LauncherNavigation navigation) {
			Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}

		private bool isBusy;
		public bool IsBusy {
			get => isBusy;
			private set => this.RaiseAndSetIfChanged(ref isBusy, value);
		}

		protected void TrackBusy(params ICommand[] commands) =>
			commands.Cast<IReactiveCommand>().Select(command => command.IsExecuting)
				.CombineLatest(running => running.Any(busy => busy))
				.Subscribe(busy => IsBusy = busy);

	}
}

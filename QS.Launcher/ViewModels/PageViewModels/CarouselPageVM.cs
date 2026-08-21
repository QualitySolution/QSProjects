using System;
using System.Threading.Tasks;
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

		#region Занятость страницы

		private int busyDepth;

		private bool isBusy; //? может сделать его volatile чтобы он был потокобезопасен
		public bool IsBusy {
			get => isBusy;
			private set => this.RaiseAndSetIfChanged(ref isBusy, value);
		}

		private string busyText;
		public string BusyText {
			get => busyText;
			protected set => this.RaiseAndSetIfChanged(ref busyText, value);
		}

		protected async Task RunBusyAsync(string title, Func<Task> operation) {
			if(operation == null)
				throw new ArgumentNullException(nameof(operation));

			if(busyDepth == 0)
				BusyText = title;

			busyDepth++;
			IsBusy = true;
			try {
				await operation();
			}
			finally {
				busyDepth--;
				if(busyDepth == 0)
					IsBusy = false;
			}
		}

		#endregion
	}
}

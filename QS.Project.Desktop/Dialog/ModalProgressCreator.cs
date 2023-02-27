using System;
using QS.Dialog.ViewModels;
using QS.Navigation;

namespace QS.Dialog {
	/// <summary>
	/// Класс реализует интерфейс IProgressBarDisplayable при этом в момент запуска прогресса, класс автоматически создает модальное
	/// окно с ходом выполнения операции. По завершении прогресса закрываем модальное окно.
	/// </summary>
	public class ModalProgressCreator : IProgressBarDisplayable {
		private readonly INavigationManager navigator;
		private IPage<ProgressWindowViewModel> activeProgressPage;

		#region Настройка
		public string Title { get; set; }
		#endregion

		protected IProgressBarDisplayable Progress => activeProgressPage.ViewModel.Progress;

		public ModalProgressCreator(INavigationManager navigator) {
			this.navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
		}
		
		#region IProgressBarDisplayable
		public void Start(double maxValue = 1, double minValue = 0, string text = null, double startValue = 0) {
			if(activeProgressPage != null)
				throw new InvalidOperationException("Прежде чем запускать новый прогресс необходимо остановить старый, вызвав Close().");

			activeProgressPage = navigator.OpenViewModel<ProgressWindowViewModel>(null);
			if(!String.IsNullOrEmpty(Title))
				activeProgressPage.ViewModel.Title = Title;
			Progress.Start(maxValue, minValue, text, startValue);
		}

		public void Update(double curValue) {
			Progress.Update(curValue);
		}

		public void Update(string curText) {
			Progress.Update(curText);
		}

		public void Add(double addValue = 1, string text = null) {
			Progress.Add(addValue, text);
		}

		public double Value => Progress.Value;
		public bool IsStarted => activeProgressPage != null;

		public void Close() {
			Progress.Close();
			navigator.ForceClosePage(activeProgressPage, CloseSource.External);
			activeProgressPage = null;
		}
		#endregion
	}
}

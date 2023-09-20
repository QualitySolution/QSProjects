using System;
using System.Collections.Generic;
using System.Threading;
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

		#region Отмена выполнения
		/// <summary>
		/// Определяем может ли пользователь закрыть диалог тем самым отменив выполнение операции.
		/// Обязательно должно быть установлено до вызова Start
		/// После вызова Start можно получить Token отмены у через свойство <c>CancellationToken</c>
		/// </summary>
		public bool UserCanCancel { get; set; } = false;

		public CancellationToken CancellationToken => activeProgressPage.ViewModel.CancellationTokenSource.Token;

		public event EventHandler Canceled;
		#endregion

		protected IProgressBarDisplayable Progress => activeProgressPage.ViewModel.Progress;

		public ModalProgressCreator(INavigationManager navigator) {
			this.navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
		}
		
		protected ModalProgressCreator() {}
		
		#region IProgressBarDisplayable
		public virtual void Start(double maxValue = 1, double minValue = 0, string text = null, double startValue = 0) {
			if(activeProgressPage != null)
				throw new InvalidOperationException("Прежде чем запускать новый прогресс необходимо остановить старый, вызвав Close().");

			activeProgressPage = navigator.OpenViewModelNamedArgs<ProgressWindowViewModel>(null, new Dictionary<string, object> {{"userCanCancel", UserCanCancel}});
			if(!String.IsNullOrEmpty(Title))
				activeProgressPage.ViewModel.Title = Title;
			activeProgressPage.PageClosed += ActiveProgressPageOnPageClosed;
			Progress.Start(maxValue, minValue, text, startValue);
		}

		private void ActiveProgressPageOnPageClosed(object sender, PageClosedEventArgs e) {
			if(e.CloseSource == CloseSource.ClosePage || e.CloseSource == CloseSource.Cancel)
				Canceled?.Invoke(this, EventArgs.Empty);
			activeProgressPage.PageClosed -= ActiveProgressPageOnPageClosed;
			activeProgressPage = null;
		}

		public virtual void Update(double curValue) {
			Progress.Update(curValue);
		}

		public void UpdateMax(double maxValue) {
			Progress.UpdateMax(maxValue);
		}

		public virtual void Update(string curText) {
			Progress.Update(curText);
		}

		public virtual void Add(double addValue = 1, string text = null) {
			Progress.Add(addValue, text);
		}

		public double Value => Progress.Value;
		public virtual bool IsStarted => activeProgressPage != null;

		public virtual void Close() {
			Progress.Close();
			navigator.ForceClosePage(activeProgressPage, CloseSource.External);
		}
		#endregion
	}
}

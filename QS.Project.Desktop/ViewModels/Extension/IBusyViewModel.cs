using System;
using System.ComponentModel;
using System.Threading;

namespace QS.ViewModels.Extension
{
	/// <summary>
	/// Состояние ViewModel, выполняющей длительную операцию.
	/// Используется представлением для управления доступностью элементов,
	/// а навигацией — для безопасной обработки попыток закрытия страницы.
	/// </summary>
	public interface IBusyViewModel : INotifyPropertyChanged
	{
		bool IsBusy { get; }
		bool CanCancelBusyOperation { get; }
		bool IsBusyCancellationRequested { get; }
		string BusyOperationTitle { get; }
		CancellationToken BusyOperationToken { get; }

		IBusyOperation BeginBusyOperation(string title = null, bool canCancel = false);
		bool RequestCancelBusyOperation();
	}

	/// <summary>
	/// Область выполнения длительной операции. При освобождении снимает с ViewModel состояние занятости.
	/// </summary>
	public interface IBusyOperation : IDisposable
	{
		CancellationToken CancellationToken { get; }
	}
}

using System;
using System.Threading;

namespace QS.Dialog {
	public interface IMainThreadDispatcher 
	{
		/// <summary>
		/// Хранит значение, определен ли главный поток в приложении
		/// </summary>
		bool HasMainThread { get; }

		/// <summary>
		/// Главный поток приложения.
		/// Если главный поток в приложении не определен, 
		/// обязан вернуть текущий поток
		/// </summary>
		Thread MainThread { get; }

		/// <summary>
		/// Запускает действие в главном потоке.
		/// Если главный поток в приложении не определен, 
		/// то обязан зпустить действие в текущем потоке
		/// </summary>
		/// <param name="action"></param>
		void RunInMainTread(Action action);
	}

}

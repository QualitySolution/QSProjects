using System;
using System.Threading;

namespace QS.Dialog
{
	public interface IGuiDispatcher : IMainThreadDispatcher
	{
		/// <summary>
		/// Позволяет получить главный поток в котором должна происходить работа с GUI
		/// </summary>
		Thread GuiThread { get; }
		/// <summary>
		/// Выполняет все накопившиеся события в GUI и возвращает управление. Обычно нужно для 
		/// перерисовки графических элементов внутри долго выполняющейся операции.
		/// </summary>
		void WaitRedraw();
		/// <summary>
		/// Позволяет вызвать из стороннего потока выполнение переданной функции в основном потоке приложения. 
		/// </summary>
		void RunInGuiTread(Action action);
		/// <summary>
		/// Позволяет дождаться выполнения какого-то условия, при этом выполняя события главного цикла приложения.
		/// </summary>
		/// <param name="checkStop">Функция для проверки что пора остановится. Если <c>true</c> выходим из ожидания.</param>
		/// <param name="sleepMilliseconds">Задержка в миллисекундах между запусками циклов проверки и обработки событий.
		/// Необходима для того чтобы не загружать CPU пустыми циклами.</param>
		void WaitInMainLoop(Func<bool> checkStop, uint sleepMilliseconds = 20);
	}
}

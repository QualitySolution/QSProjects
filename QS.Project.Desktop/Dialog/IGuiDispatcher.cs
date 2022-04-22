using System;
using System.Threading;

namespace QS.Dialog
{
	public interface IGuiDispatcher
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
		/// Позволяет дождаться выполнения какого-то условия, при этом выполняя события главного цикла
		/// приложения.
		/// <paramref name="checkStop">Функция для проверки что пора остановится.</paramref>
		/// <paramref name="sleepMilliseconds"/>Задержка в миллисекундах между запусками циклов проверки и обработки событий.
		/// Необходима для того чтобы не загружать CPU пустыми циклами.
		/// </summary>
		void WaitInMainLoop(Func<bool> checkStop, uint sleepMilliseconds = 20);
	}
}

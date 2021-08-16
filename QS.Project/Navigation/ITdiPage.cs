using QS.Tdi;

namespace QS.Navigation
{
	/// <summary>
	/// Интерфейс специально созданный на переходный период, пока есть микс из диалогов разных типов Tdi и ViewModel
	/// </summary>
	public interface ITdiPage : IPage
	{
		ITdiTab TdiTab { get; }
	}
}
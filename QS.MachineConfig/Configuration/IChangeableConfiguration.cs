using System;
namespace QS.Configuration
{
	/// <summary>
	/// Интерфейс который в плане чтения конфигурации должен быть похожим на IConfiguration от MS
	/// Чтобы он был заменяем без переписывания кода и в последствии
	/// И был бы как IConfiguration более универсальным. То есть не связанным с местом хранения данных.
	/// </summary>
	public interface IChangeableConfiguration
	{
		/// <summary>
		/// Можно получать и устанавливать значение через Item[key]
		/// </summary>
		/// <param name="key">Key.</param>
		string this[string key] { get; set; }

		void Reload();
	}
}

namespace QS.Dialog
{
	/// <summary>
	/// Интерфейс объединяет 2 интерфейса IInteractiveMessage, IInteractiveQuestion вывода простых сообщений.
	/// Удобно использовать в случае если нужно и то и другое. Можно получить один класс в качетве зависимости.
	/// </summary>
	public interface IInteractiveService : IInteractiveMessage, IInteractiveQuestion
	{
	}
}
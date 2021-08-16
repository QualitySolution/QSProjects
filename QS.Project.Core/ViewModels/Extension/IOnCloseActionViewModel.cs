using System;
using QS.Navigation;

namespace QS.ViewModels.Extension
{
	/// <summary>
	/// При добавлении этого интерфейса на ViewModel. ViewModel сможет прореагировать на событие ее закрытия.
	/// ViewModel получить вызов перед получение события о закрытии страницы внешними подписчиками, то есть ViewModel
	/// имеет возможность перед закрытием подготовить данные. По этой причине в методе нельзя производить деструктиные действия
	/// освобождения ресурсов и т.п. Для освобождения ресурсов реализуйте интерфейс IDisposable. А для остановки закрытия 
	/// реализуйте аналог ITDICloseControlTab 
	/// </summary>
	public interface IOnCloseActionViewModel
	{
		void OnClose(CloseSource source);
	}
}

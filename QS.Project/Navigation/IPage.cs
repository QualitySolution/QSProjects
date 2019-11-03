using System;
using System.Collections.Generic;
using QS.ViewModels;

namespace QS.Navigation
{
	public interface IPage
	{
		/// <summary>
		/// Хеш страницы, используется для поиска запрошенной страницы среди уже открытых и переключения на ее вместо создания новой.
		/// Хеш = null, значит страница не должна проверятся на дубликат.
		/// </summary>
		string PageHash { get; }

		ViewModelBase ViewModel { get; }

		event EventHandler PageClosed;

		List<IPage> SlavePages { get; }
		List<IPage> ChildPages { get; }
	}

	internal interface IPageInternal
	{
		void OnClosed();
	}

	public interface IPage<TViewModel> : IPage
		where TViewModel : ViewModelBase
	{
		new TViewModel ViewModel { get; }
	}
}

﻿using System;
using System.Collections.Generic;
using QS.Tdi;
using QS.ViewModels.Dialog;

namespace QS.Navigation
{
	public interface IPage
	{
		/// <summary>
		/// Хеш страницы, используется для поиска запрошенной страницы среди уже открытых и переключения на ее вместо создания новой.
		/// Хеш = null, значит страница не должна проверятся на дубликат.
		/// </summary>
		string PageHash { get; }

		DialogViewModelBase ViewModel { get; }

		/// <summary>
		/// Для хранения пользовательской информации как в WinForms
		/// </summary>
		object Tag { get; set; }

		event EventHandler<PageClosedEventArgs> PageClosed;

		IEnumerable<MasterToSlavePair> SlavePagesAll { get; }
		IEnumerable<ParentToChildPair> ChildPagesAll { get; }

		IEnumerable<IPage> SlavePages { get; }
		IEnumerable<IPage> ChildPages { get; }
	}

	internal interface IPageInternal
	{
		void OnClosed(CloseSource source);
		void AddSlavePage(IPage page);
		bool RemoveSlavePage(IPage page);
		void AddChildPage(IPage page);
		bool RemoveChildPage(IPage page);
	}

	/// <summary>
	/// Интерфейс специально созданный на переходный период, пока есть микс из диалогов разных типов Tdi и ViewModel
	/// </summary>
	public interface ITdiPage : IPage
	{
		ITdiTab TdiTab { get; }
	}

	public interface IPage<TViewModel> : IPage
		where TViewModel : DialogViewModelBase
	{
		new TViewModel ViewModel { get; }
	}

	public class MasterToSlavePair
	{
		public IPage MasterPage;
		public IPage SlavePage;
	}

	public class ParentToChildPair
	{
		public IPage ParentPage;
		public IPage ChildPage;
	}

	public class PageClosedEventArgs : EventArgs
	{
		public CloseSource CloseSource { get; }

		public PageClosedEventArgs(CloseSource source)
		{
			CloseSource = source;
		}
	}
}

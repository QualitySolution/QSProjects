using System;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Navigation
{
	/// <summary>
	/// Этот класс используется ТОЛЬКО для страниц с ITdiTab без ViewModel, чтобы такие страницы можно было указывать как мастер.
	/// </summary>
	public class TdiTabPage : PageBase, IPage, ITdiPage
	{
		public TdiTabPage(ITdiTab tab, string hash)
		{
			TdiTab = tab;
			PageHash = hash;
		}

		public ITdiTab TdiTab { get; protected set; }
		public override string Title => TdiTab?.TabName;
	}
}

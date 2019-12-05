using System;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Navigation
{
	public class TdiTabPage : PageBase, IPage, ITdiPage
	{
		public TdiTabPage(ITdiTab tab)
		{
			TdiTab = tab;
		}

		public ITdiTab TdiTab { get; protected set; }
	}
}

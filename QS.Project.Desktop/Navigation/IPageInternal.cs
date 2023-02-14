using System;
using System.Collections.Generic;
using QS.ViewModels.Dialog;

namespace QS.Navigation
{
	internal interface IPageInternal
	{
		void OnClosed(CloseSource source);
		void AddSlavePage(IPage page);
		bool RemoveSlavePage(IPage page);
		void AddChildPage(IPage page);
		bool RemoveChildPage(IPage page);
	}
}

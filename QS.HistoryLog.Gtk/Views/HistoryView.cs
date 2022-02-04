using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Widgets;
using QS.HistoryLog.Domain;
using QS.HistoryLog.ViewModels;
using QS.Utilities;
using QS.Views;
using QS.Views.Dialog;
using QSOrmProject;
using QSProjectsLib;
using QSWidgetLib;

namespace QS.HistoryLog.Views
{
	public partial class HistoryView : DialogViewBase<HistoryViewModel>
	{
		public HistoryView(HistoryViewModel viewModel): base(viewModel)
		{
			this.Build();
		}
	}
}
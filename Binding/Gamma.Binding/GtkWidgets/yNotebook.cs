using System;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma Gtk")]
	public class yNotebook : Notebook
	{
		public BindingControler<yNotebook> Binding { get; private set; }

		public yNotebook()
		{
			Binding = new BindingControler<yNotebook>(this, new Expression<Func<yNotebook, object>>[] {
				(w => w.CurrentPage)
			});
		}

		protected override void OnSwitchPage(NotebookPage page, uint page_num)
		{
			base.OnSwitchPage(page, page_num);
			Binding.FireChange(w => w.CurrentPage);
		}
	}
}

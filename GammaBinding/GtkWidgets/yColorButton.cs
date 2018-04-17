using System;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma Gtk")]
	public class yColorButton : ColorButton
	{
		public BindingControler<yColorButton> Binding { get; private set; }

		public yColorButton()
		{
			Binding = new BindingControler<yColorButton>(this, new Expression<Func<yColorButton, object>>[] {
				(w => w.Color)
			});
		}

		public yColorButton(object tag) : this()
		{
			Tag = tag;
		}

		/// <summary>
		/// For store user data, like WinForms Tag.
		/// </summary>
		public object Tag;

		protected override void OnColorSet()
		{
			Binding.FireChange(w => w.Color);
			base.OnColorSet();
		}
	}
}

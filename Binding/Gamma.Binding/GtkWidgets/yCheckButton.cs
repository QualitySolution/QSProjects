using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yCheckButton : CheckButton
	{
		public BindingControler<yCheckButton> Binding { get; private set; }

		public yCheckButton ()
		{
			Binding = new BindingControler<yCheckButton> (this, new Expression<Func<yCheckButton, object>>[] {
				(w => w.Active)
			});
		}

		public yCheckButton(object tag) : this()
		{
			Tag = tag;
		}

		/// <summary>
		/// For store user data, like WinForms Tag.
		/// </summary>
		public object Tag;

		protected override void OnToggled ()
		{
			Binding.FireChange (w => w.Active);
			base.OnToggled ();
		}
	}
}


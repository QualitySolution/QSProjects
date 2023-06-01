using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yTextView : TextView
	{
		public BindingControler<yTextView> Binding { get; private set;}

		public yTextView ()
		{
			Binding = new BindingControler<yTextView> (this, new Expression<Func<yTextView, object>>[] {
				(w => w.Buffer.Text)
			});
			Buffer.Changed += Buffer_Changed;
		}

		void Buffer_Changed (object sender, EventArgs e)
		{
			Binding.FireChange (w => w.Buffer.Text);
		}
	}
}


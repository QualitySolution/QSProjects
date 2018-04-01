using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yFileChooserButton : FileChooserButton
	{
		public BindingControler<yFileChooserButton> Binding { get; private set;}

		public new string Filename
		{
			get => base.Filename;
			set => base.SetFilename(value);
		}

		public yFileChooserButton() : base((Widget)null) {
			Binding = new BindingControler<yFileChooserButton>(this, new Expression<Func<yFileChooserButton, object>>[] {
				w => w.Filename,
				w => w.Filenames
			});
		}

        protected override void OnSelectionChanged()
        {
			Binding.FireChange(w => w.Filename, w => w.Filenames);
			base.OnSelectionChanged();
        }
    }
}


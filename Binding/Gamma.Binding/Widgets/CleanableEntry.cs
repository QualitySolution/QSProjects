using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;

namespace Gamma.Widgets {
	[ToolboxItem(true)]
	[Category("Gamma Widgets")]
	public partial class CleanableEntry : Gtk.Bin {
		public BindingControler<CleanableEntry> Binding { get; private set; }

		public CleanableEntry() {
			this.Build();
			Binding = new BindingControler<CleanableEntry>(this, new Expression<Func<CleanableEntry, object>>[] {
				(w => w.Text)
			});
		}

		#region Свойства
		public string Text {
			get => entry.Text;
			set { 
				if(entry.Text != value)
					entry.Text = value;
			}
		}
		#endregion

		#region События
		protected void OnEntryChanged(object sender, EventArgs e) {
			Binding.FireChange(w => w.Text);
		}

		protected void OnButtonClearClicked(object sender, EventArgs e) {
			Text = String.Empty;
		}
		#endregion
	}
}

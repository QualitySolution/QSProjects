using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.Widgets {
	[ToolboxItem(true)]
	[Category("Gamma Widgets")]
	public partial class CleanableEntry : Gtk.Bin {
		public BindingControler<CleanableEntry> Binding { get; private set; }
		public event EventHandler Changed;

		public CleanableEntry() {
			this.Build();
			Binding = new BindingControler<CleanableEntry>(this, new Expression<Func<CleanableEntry, object>>[] {
				(w => w.Text)
			});
			entry.Changed += EntryChanged;
			entry.TextInserted += EntryTextInserted;
			entry.TextDeleted += EntryTextDeleted;
		}

		private void EntryTextDeleted(object sender, TextDeletedArgs e) {
			Changed?.Invoke(sender, e);
			OnEntryChanged(sender, e);
		}

		private void EntryTextInserted(object sender, TextInsertedArgs e) {
			Changed?.Invoke(sender, e);
			OnEntryChanged(sender, e);
		}

		private void EntryChanged(object sender, EventArgs e) {
			Changed?.Invoke(sender, e);
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

		protected override void OnDestroyed() {
			entry.Changed -= EntryChanged;
			entry.TextInserted -= EntryTextInserted;
			entry.TextDeleted -= EntryTextDeleted;
			base.OnDestroyed();
		}

		public void SetColor(StateType stateType, Gdk.Color color) {
			entry.ModifyText(stateType, color);
		}
	}
}

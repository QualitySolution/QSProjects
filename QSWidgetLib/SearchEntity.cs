using System;
using Gtk;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SearchEntity : Bin
	{
		public event EventHandler TextChanged;

		public string Text {
			get{
				return entrySearchText.Text;
			}
			set{
				entrySearchText.Text = value;
			}
		}

		public SearchEntity()
		{
			this.Build();
		}

		protected void OnButtonClearClicked(object sender, EventArgs e)
		{
			entrySearchText.Text = String.Empty;
		}

		protected void OnEntrySearchTextChanged(object sender, EventArgs e)
		{
			if (TextChanged != null)
				TextChanged(this, EventArgs.Empty);
		}

		protected override void OnDestroyed() {
			var clearImage = buttonClear.Image as Image;
			clearImage.Pixbuf.Dispose();
			clearImage.Pixbuf = null;
			base.OnDestroyed();
		}
	}
}


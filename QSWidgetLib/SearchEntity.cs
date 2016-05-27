using System;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SearchEntity : Gtk.Bin
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
	}
}


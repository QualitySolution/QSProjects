using System;
using Gtk;
using System.Collections.Generic;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public class CheckList : Gtk.VBox
	{
		public Dictionary<string, Gtk.CheckButton> CheckButtons;

		public event EventHandler<CheckListClickedEventArgs> CheckListClicked;

		public CheckList()
		{
			CheckButtons = new Dictionary<string, CheckButton>();
		}

		public void AddCheckButton(string key, string label, string tooltip = "", bool active = false)
		{
			Gtk.CheckButton CheckBox = new CheckButton();
			CheckBox.Label = label;
			if(tooltip != "")
				CheckBox.TooltipText = tooltip;
			CheckBox.Active = active;
			CheckBox.Clicked += OnCheckClicked;
			CheckButtons.Add (key, CheckBox);
			this.PackStart (CheckBox, false, false, 0);
			this.ShowAll();
		}

		public int SelectedCount
		{
			get{
				int count = 0;
				foreach(KeyValuePair<string, CheckButton> pair in CheckButtons)
				{
					if (pair.Value.Active)
						count++;
				}
				return count;
			}
		}

		protected void OnCheckClicked (object sender, EventArgs e)
		{
			if(CheckListClicked != null)
			{
				CheckButton button = (CheckButton)sender;
				CheckListClickedEventArgs arg = new CheckListClickedEventArgs();
				arg.Button = button;
				foreach(KeyValuePair<string, CheckButton> pair in CheckButtons)
				{
					if (pair.Value == button)
						arg.Key = pair.Key;
				}
				CheckListClicked(this, arg);
			}
				
		}

	}

	public class CheckListClickedEventArgs : EventArgs
	{
		public string Key { get; set; }
		public CheckButton Button { get; set; }
	}

}


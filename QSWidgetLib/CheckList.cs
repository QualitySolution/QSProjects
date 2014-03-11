using System;
using Gtk;
using System.Collections.Generic;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public class CheckList : Gtk.VBox
	{
		public Dictionary<string, Gtk.CheckButton> CheckButtons;

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
	}
}


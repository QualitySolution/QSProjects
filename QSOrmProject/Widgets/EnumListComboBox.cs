using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;

namespace QSOrmProject
{
	[ToolboxItem (true)]
	[Category ("QS Widgets")]
	public class EnumListComboBox : ComboBox
	{
		Type enumType;
		ListStore comboListStore;

		public event EventHandler<EnumItemClickedEventArgs> EnumItemSelected;

		public void SetEnumItems<T> (IList<T> itemsToShow)
		{
			comboListStore.Clear();
			enumType = typeof(T);
			if (!enumType.IsEnum)
				throw new NotSupportedException (string.Format ("EnumItems only supports enum types, specified was {0}", enumType));
			foreach (FieldInfo fi in enumType.GetFields()) {
				AppendEnumItem<T> (fi, itemsToShow);
			}
		}

		void AppendEnumItem<T> (FieldInfo info, IList<T> itemsToShow)
		{
			if (info.Name.Equals ("value__"))
				return;
			if (!itemsToShow.Contains ((T)info.GetValue (null)))
				return;
			string item = info.GetEnumTitle ();
			comboListStore.AppendValues (info.GetValue (null), item);
		}

		void OnEnumItemSelected ()
		{
			if (EnumItemSelected != null) {
				EnumItemSelected (this, new EnumItemClickedEventArgs (SelectedItem));
			}
		}

		protected override void OnChanged ()
		{
			base.OnChanged ();
			TreeIter iter;

			if (GetActiveIter (out iter)) {
				SelectedItem = Model.GetValue (iter, 0);
			} else {
				SelectedItem = SpecialComboState.None;
			}
		}

		object selectedItem;

		public object SelectedItem {
			get {
				return selectedItem;
			}
			set {
				if (selectedItem == value)
					return;
				selectedItem = value;
				OnEnumItemSelected ();
			}
		}

		public EnumListComboBox ()
		{
			comboListStore = new ListStore (typeof(object), typeof(string));
			CellRendererText text = new CellRendererText ();
			Model = comboListStore;
			PackStart (text, false);
			AddAttribute (text, "text", 1);
		}
	}
}


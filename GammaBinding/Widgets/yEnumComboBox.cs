using System;
using System.ComponentModel;
using System.Reflection;
using Gtk;
using System.Collections.Generic;
using Gamma.Utilities;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using Gamma.GtkHelpers;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	public class yEnumComboBox : ComboBox
	{
		ListStore comboListStore;

		enum comboDataColumns {
			Title,
			Item
		}

		List<object> fieldsToHide = new List<object> ();

		public BindingControler<yEnumComboBox> Binding { get; private set;}

		public void AddEnumToHideList (object[] items)
		{
			fieldsToHide.AddRange (items);
			ResetLayout ();
		}

		public void RemoveEnumFromHideList (object[] items)
		{
			foreach (object item in items)
				if (fieldsToHide.Contains (item))
					fieldsToHide.Remove (item);
			ResetLayout ();
		}

		public void ClearEnumHideList ()
		{
			fieldsToHide.Clear ();
			ResetLayout ();
		}

		public event EventHandler<ItemSelectedEventArgs> EnumItemSelected;

		[Browsable (true)]
		public String ItemsEnumName {
			get {
				return ItemsEnum.AssemblyQualifiedName;
			}
			set {
				if (String.IsNullOrEmpty (value))
					return;

				ItemsEnum = Type.GetType (value);
				if (ItemsEnum == null)
					Console.WriteLine (String.Format ("{0} is not exist. Property ItemsEnumName must have format '<namespace>.<enumtype>, <Assembly>'", value));
			}
		}

		Type itemsEnum;

		public Type ItemsEnum {
			get {
				return itemsEnum;
			}
			set {
				if (itemsEnum == value)
					return;
				itemsEnum = value;

				ResetLayout ();
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

				TreeIter iter;
				if (!ListStoreHelper.SearchListStore (comboListStore, value, (int)comboDataColumns.Item, out iter))
					return;

				selectedItem = value;
				SetActiveIter (iter);

				Binding.FireChange (				
					(w => w.Active),
					(w => w.ActiveText),
					(w => w.SelectedItem),
					(w => w.SelectedItemOrNull));
				OnEnumItemSelected ();
			}
		}

		public object SelectedItemOrNull {
			get {
				return selectedItem is SpecialComboState ? null : selectedItem;
			}
			set {
				if (value == null) {
					if (ShowSpecialStateNot)
						SelectedItem = SpecialComboState.Not;
					else if(ShowSpecialStateAll)
						SelectedItem = SpecialComboState.All;
					else
						SelectedItem = SpecialComboState.None;
				} 
				else
					SelectedItem = value;
			}
		}

		bool showSpecialStateAll;

		[Browsable (true)]
		public bool ShowSpecialStateAll {
			get {
				return showSpecialStateAll;
			}
			set {
				showSpecialStateAll = value;
				ResetLayout ();
			}
		}

		bool showSpecialStateNot;

		[Browsable (true)]
		public bool ShowSpecialStateNot {
			get {
				return showSpecialStateNot;
			}
			set {
				showSpecialStateNot = value;
				ResetLayout ();
			}
		}

		public yEnumComboBox ()
		{
			Binding = new BindingControler<yEnumComboBox> (this, new Expression<Func<yEnumComboBox, object>>[] {
				(w => w.Active),
				(w => w.ActiveText),
				(w => w.SelectedItem),
				(w => w.SelectedItemOrNull)
			});

			comboListStore = new ListStore (typeof(string), typeof(object));
			CellRendererText text = new CellRendererText ();
			PackStart (text, false);
			AddAttribute (text, "text", (int)comboDataColumns.Title);
			Model = comboListStore;
		}

		void ResetLayout ()
		{
			comboListStore.Clear ();

			if (ItemsEnum == null)
				return;

			if (!ItemsEnum.IsEnum)
				throw new NotSupportedException (string.Format ("ItemsEnum only supports enum types, specified was {0}", ItemsEnum));

			//Fill special fields
			if (ShowSpecialStateAll) {
				AppendEnumItem (typeof(SpecialComboState).GetField ("All"));
			}
			if (ShowSpecialStateNot) {
				AppendEnumItem (typeof(SpecialComboState).GetField ("Not"));
			}

			foreach (FieldInfo info in ItemsEnum.GetFields()) {
				AppendEnumItem (info);
			}

			if (ShowSpecialStateAll || ShowSpecialStateNot)
				Active = 0;
		}

		void AppendEnumItem (FieldInfo info)
		{
			if (info.Name.Equals ("value__"))
				return;
			if (fieldsToHide.Contains (info.GetValue (null)))
				return;
			string item = info.GetEnumTitle ();
			comboListStore.AppendValues (item, info.GetValue (null));
		}

		void OnEnumItemSelected ()
		{
			if (EnumItemSelected != null) {
				EnumItemSelected (this, new ItemSelectedEventArgs (SelectedItem));
			}
		}

		protected override void OnChanged ()
		{
			TreeIter iter;

			if (GetActiveIter (out iter)) {
				SelectedItem = Model.GetValue (iter, (int)comboDataColumns.Item);
			} else {
				SelectedItem = SpecialComboState.None;
			}
			base.OnChanged ();
		}
	}

}


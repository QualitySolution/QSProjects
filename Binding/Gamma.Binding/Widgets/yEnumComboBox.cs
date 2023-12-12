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
	[ToolboxItem(true)]
	[Category("Gamma Widgets")]
	public class yEnumComboBox : ComboBox {
		ListStore comboListStore;
		private bool _destroyed;

		enum comboDataColumns
		{
			Title,
			Item
		}

		public BindingControler<yEnumComboBox> Binding { get; private set; }

		#region HideItems
		List<object> fieldsToHide = new List<object>();

		public void AddEnumToHideList(params object[] items)
		{
			fieldsToHide.AddRange(items);
			ResetLayout();
		}
		
		public void AddEnumerableToHideList<T>(IEnumerable<T> items)
		{
			foreach(var item in items) {
				fieldsToHide.Add(item);
			}
			ResetLayout();
		}

		public void RemoveEnumFromHideList(params object[] items)
		{
			foreach(object item in items)
				if(fieldsToHide.Contains(item))
					fieldsToHide.Remove(item);
			ResetLayout();
		}

		public void ClearEnumHideList()
		{
			fieldsToHide.Clear();
			ResetLayout();
		}

		public object[] HiddenItems {
			get => fieldsToHide.ToArray(); 
			set {
				fieldsToHide.Clear();
				AddEnumToHideList(value);
			}
		}

		#endregion

		public event EventHandler<ItemSelectedEventArgs> EnumItemSelected;

		bool IsNotUserChange;
		public event EventHandler ChangedByUser;

		Type itemsEnum;

		public Type ItemsEnum {
			get {
				return itemsEnum;
			}
			set {
				if(itemsEnum == value)
					return;
				itemsEnum = value;

				ResetLayout();
			}
		}

		bool defaultFirst;

		/// <summary>
		/// If true combo will select first item by default, insted of empty combo state.
		/// </summary>
		[DefaultValue(false)]
		public bool DefaultFirst {
			get {
				return defaultFirst;
			}
			set {
				defaultFirst = value;
			}
		}

		object selectedItem;

		/// <summary>
		/// Return selected values or SpecialComboState enum.
		/// </summary>
		/// <value>The selected item.</value>
		public object SelectedItem {
			get {
				return selectedItem;
			}
			set {
				if(selectedItem == value || (selectedItem != null && selectedItem.Equals(value))) //Second expression needed to correct check enums value wrapped in object
					return;

				TreeIter iter;
				if(!ListStoreHelper.SearchListStore(comboListStore, value, (int)comboDataColumns.Item, out iter))
					return;

				IsNotUserChange = true;
				selectedItem = value;
				SetActiveIter(iter);

				Binding.FireChange(
					(w => w.Active),
					(w => w.ActiveText),
					(w => w.SelectedItem),
					(w => w.SelectedItemOrNull));
				OnEnumItemSelected();
				IsNotUserChange = false;
			}
		}

		public object SelectedItemOrNull {
			get {
				return selectedItem is SpecialComboState ? null : selectedItem;
			}
			set {
				if(value == null) {
					if(ShowSpecialStateNot)
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

		bool useShortTitle;

		[Browsable(true)]
		public bool UseShortTitle {
			get {
				return useShortTitle;
			}
			set {
				useShortTitle = value;
				ResetLayout();
			}
		}

		bool showSpecialStateAll;

		[Browsable(true)]
		public bool ShowSpecialStateAll {
			get {
				return showSpecialStateAll;
			}
			set {
				showSpecialStateAll = value;
				ResetLayout();
			}
		}

		bool showSpecialStateNot;

		[Browsable(true)]
		public bool ShowSpecialStateNot {
			get {
				return showSpecialStateNot;
			}
			set {
				showSpecialStateNot = value;
				ResetLayout();
			}
		}

		public yEnumComboBox()
		{
			Binding = new BindingControler<yEnumComboBox>(this, new Expression<Func<yEnumComboBox, object>>[] {
				(w => w.Active),
				(w => w.ActiveText),
				(w => w.SelectedItem),
				(w => w.SelectedItemOrNull)
			});

			comboListStore = new ListStore(typeof(string), typeof(object));
			CellRendererText text = new CellRendererText();
			PackStart(text, false);
			AddAttribute(text, "text", (int)comboDataColumns.Title);
			Model = comboListStore;
		}

		void ResetLayout()
		{
			selectedItem = null;
			comboListStore.Clear();

			if(ItemsEnum == null)
				return;

			if(!ItemsEnum.IsEnum)
				throw new NotSupportedException(string.Format("ItemsEnum only supports enum types, specified was {0}", ItemsEnum));

			//Fill special fields
			if(ShowSpecialStateAll) {
				AppendEnumItem(typeof(SpecialComboState).GetField("All"));
			}
			if(ShowSpecialStateNot) {
				AppendEnumItem(typeof(SpecialComboState).GetField("Not"));
			}

			foreach(FieldInfo info in ItemsEnum.GetFields()) {
				AppendEnumItem(info);
			}

			if(ShowSpecialStateAll || ShowSpecialStateNot || DefaultFirst) {
				IsNotUserChange = true;
				Active = 0;
				IsNotUserChange = false;
			}
		}

		void AppendEnumItem(FieldInfo info)
		{
			if(info.Name.Equals("value__"))
				return;
			if(fieldsToHide.Contains(info.GetValue(null)))
				return;
			string item = UseShortTitle ? info.GetShortTitle() : info.GetEnumTitle();
			comboListStore.AppendValues(item, info.GetValue(null));
		}

		void OnEnumItemSelected()
		{
			if(EnumItemSelected != null) {
				EnumItemSelected(this, new ItemSelectedEventArgs(SelectedItem));
			}
		}

		protected override void OnChanged()
		{
			TreeIter iter;
			bool isNotUserChange = IsNotUserChange;

			if(GetActiveIter(out iter)) {
				SelectedItem = Model.GetValue(iter, (int)comboDataColumns.Item);
			}
			else {
				SelectedItem = SpecialComboState.None;
			}
			base.OnChanged();
			if(!isNotUserChange && ChangedByUser != null) {
				ChangedByUser(this, EventArgs.Empty);
			}
		}

		protected override void OnDestroyed() {
			if(_destroyed) {
				return;
			}

			Binding.CleanSources();
			Model = null;
			comboListStore.Clear();
			comboListStore.Dispose();
			base.OnDestroyed();
			
			_destroyed = true;
		}
	}
}

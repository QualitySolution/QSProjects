using Gamma.Binding.Core;
using Gamma.GtkHelpers;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using QS.DomainModel.Entity;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;

namespace QS.Widgets.GtkUI {
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public class SpecialListComboBox : ComboBox {

		private ListStore _comboListStore;
		private bool _destroyed;
		private bool _notUserChanage = false;
		private bool _defaultFirst;
		private object _selectedItem;

		public SpecialListComboBox() {
			SelectedItemStrictTyped = true;

			Binding = new BindingControler<SpecialListComboBox>(this, new Expression<Func<SpecialListComboBox, object>>[] {
				w => w.Active,
				w => w.ActiveText,
				w => w.SelectedItem
			});

			_comboListStore = new ListStore(typeof(string), typeof(object));
			var text = new CellRendererText();
			PackStart(text, false);
			AddAttribute(text, "text", (int)comboDataColumns.Title);
			Model = _comboListStore;

			RenderTextFunc = RenderText;
		}

		public event EventHandler<ItemSelectedEventArgs> ItemSelected;

		/// <summary>
		/// При установки этого значения в true SelectedItem будет возвращать только значения из списка.
		/// Служебные значения enum-а SpecialComboState будут преобразовываться в null.
		/// </summary>
		public bool SelectedItemStrictTyped { get; set; }

		public Func<object, string> RenderTextFunc { get;set; }

		public BindingControler<SpecialListComboBox> Binding { get; private set; }

		public object SelectedItem {
			get {
				if(SelectedItemStrictTyped)
					return _selectedItem is SpecialComboState ? null : _selectedItem;
				else
					return _selectedItem;
			}
			set {
				if(_selectedItem == value)
					return;

				TreeIter iter;
				if(value == null) {
					if(ShowSpecialStateAll)
						_selectedItem = SpecialComboState.All;
					else if(ShowSpecialStateNot)
						_selectedItem = SpecialComboState.Not;
					else
						_selectedItem = null;
				}
				if(value is IDomainObject) {
					if(ListStoreHelper.SearchListStore<IDomainObject>((ListStore)Model, item => item.Id == ((IDomainObject)value).Id, 1, out iter)) {
						_selectedItem = ((ListStore)Model).GetValue(iter, (int)comboDataColumns.Item);
					}
					else if(AddIfNotExist)
						_selectedItem = value;
				}
				else {
					_selectedItem = value;
				}
			}
		}

		public bool IsSelectedAll {
			get {
				return SpecialComboState.All.Equals(SelectedItem);
			}
		}

		public bool IsSelectedNot {
			get {
				return SpecialComboState.Not.Equals(SelectedItem);
			}
		}

		private bool showSpecialStateAll = false;
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

		private bool showSpecialStateNot = false;
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

		[Browsable(true)]
		public string NameForSpecialStateNot { get; set; } = null;

		private IEnumerable itemsList;
		public IEnumerable ItemsList {
			get {
				if(ShowSpecialStateAll)
					yield return SpecialComboState.All;
				if(ShowSpecialStateNot)
					yield return SpecialComboState.Not;
				if(itemsList == null)
					yield break;
				foreach(var item in itemsList)
					yield return item;
			}
			set {
				if(itemsList == value)
					return;
				itemsList = value;
				ResetLayout();
			}
		}

		/// <summary>
		/// If true combo will select first item by default, insted of empty combo state.
		/// </summary>
		[DefaultValue(false)]
		public bool DefaultFirst {
			get {
				return _defaultFirst;
			}
			set {
				_defaultFirst = value;
			}
		}

		[Browsable(true)]
		[DefaultValue(false)]
		public bool AddIfNotExist { get; set; }
		
		public void SetRenderTextFunc<TObject>(Func<TObject, string> renderTextFunc) {
			RenderTextFunc = o => renderTextFunc((TObject)o);
			ResetLayout();
		}

		protected void ResetLayout() {
			if(ShowSpecialStateAll || ShowSpecialStateNot)
				Active = 0;
		}

		protected override void OnChanged() {
			if(_notUserChanage)
				return;

			TreeIter iter;

			if(GetActiveIter(out iter)) {
				SelectedItem = UnWrapValueIfNeed(Model.GetValue(iter, (int)comboDataColumns.Item));
			}
			else {
				SelectedItem = null;
			}
			base.OnChanged();
		}

		protected object WrapValueIfNeed(object value) {
			if(value is string || value is int || value is uint || value is decimal || value is double || value is long || value is ulong || value is short || value is ushort)
				return new KeepInObject(value);
			else
				return value;
		}

		protected object UnWrapValueIfNeed(object value) {
			return value is KeepInObject keepInObject ? keepInObject.Value : value;
		}

		protected override void OnDestroyed() {
			if(_destroyed) {
				return;
			}

			Binding.CleanSources();
			Model = null;
			_comboListStore.Clear();
			_comboListStore.Dispose();
			base.OnDestroyed();

			_destroyed = true;
		}

		private string RenderText(object item) {
			if(item is SpecialComboState specialState) {
				if(!string.IsNullOrEmpty(NameForSpecialStateNot) && specialState == SpecialComboState.Not)
					return NameForSpecialStateNot;
				return specialState.GetEnumTitle();
			}

			if(RenderTextFunc != null)
				return RenderTextFunc(item);

			return DomainHelper.GetTitle(item);
		}

		//HACK workaround can not save to object column of ListStore simple types
		protected struct KeepInObject {
			public object Value;

			public KeepInObject(object value) {
				Value = value;
			}
		}

		protected enum comboDataColumns {
			Title,
			Item
		}
	}
}

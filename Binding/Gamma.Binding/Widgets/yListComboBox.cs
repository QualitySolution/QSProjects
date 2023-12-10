using System;
using System.ComponentModel;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using Gamma.GtkHelpers;
using System.Collections;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	public class yListComboBox : ComboBox
	{
		ListStore comboListStore;
		private bool _destroyed;

		bool notUserChanage = false;

		protected enum comboDataColumns {
			Title,
			Item
		}

		[Browsable(true)]
		[DefaultValue(false)]
		public bool AddIfNotExist { get; set;}

		IEnumerable itemsList;
		public virtual IEnumerable ItemsList {
			get {
				return itemsList;
			}
			set {
				if (itemsList == value)
					return;
				itemsList = value;

				ResetLayout ();
			}
		}

		bool defaultFirst;

		/// <summary>
		/// If true combo will select first item by default, insted of empty combo state.
		/// </summary>
		[DefaultValue (false)]
		public bool DefaultFirst {
			get {
				return defaultFirst;
			}
			set {
				defaultFirst = value;
			}
		}

		public Func<object, string> RenderTextFunc;

		public virtual void SetRenderTextFunc<TObject> (Func<TObject, string> renderTextFunc)
		{
			RenderTextFunc = o => renderTextFunc ((TObject)o);
		}

		public BindingControler<yListComboBox> Binding { get; private set;}

		public event EventHandler<ItemSelectedEventArgs> ItemSelected;

		object selectedItem;

		public virtual object SelectedItem {
			get {
				return selectedItem;
			}
			set {
				if (selectedItem == value)
					return;

				TreeIter iter = TreeIter.Zero;
				if (value != null && !ListStoreHelper.SearchListStore<object> (comboListStore, 
					o => value.Equals (UnWrapValueIfNeed(o)), (int)comboDataColumns.Item, out iter))
				{
					if (AddIfNotExist == false)
						return;

					iter = comboListStore.AppendValues (
						RenderTextFunc == null ? value.ToString () : RenderTextFunc(value),
						WrapValueIfNeed (value)
					);
				}

				selectedItem = value;

				notUserChanage = true;
				if (value == null)
					Active = -1;
				else
					SetActiveIter (iter);
				notUserChanage = false;

				Binding.FireChange(
					(w => w.Active),
					(w => w.ActiveText),
					(w => w.SelectedItem));
				OnEnumItemSelected ();
			}
		}
		
		public yListComboBox ()
		{
			Binding = new BindingControler<yListComboBox> (this, new Expression<Func<yListComboBox, object>>[] {
				(w => w.Active),
				(w => w.ActiveText),
				(w => w.SelectedItem)
			});

			comboListStore = new ListStore (typeof(string), typeof(object));
			CellRendererText text = new CellRendererText ();
			PackStart (text, false);
			AddAttribute (text, "text", (int)comboDataColumns.Title);
			Model = comboListStore;
		}

		protected virtual void ResetLayout ()
		{
			comboListStore.Clear();

			if (ItemsList == null)
				return;

			foreach (var item in ItemsList) {
				if (item == null)
					continue;
				var title = RenderTextFunc == null ? item.ToString() : RenderTextFunc(item);
				if (title == null)
					continue;
				comboListStore.AppendValues (
					title,
					WrapValueIfNeed (item)
				);
			}
			if (DefaultFirst && comboListStore.IterNChildren() > 0)
				Active = 0;
		}

		void OnEnumItemSelected ()
		{
			if (ItemSelected != null) {
				ItemSelected (this, new ItemSelectedEventArgs (SelectedItem));
			}
		}

		protected override void OnChanged ()
		{
			if (notUserChanage)
				return;

			TreeIter iter;

			if (GetActiveIter (out iter)) {
				SelectedItem = UnWrapValueIfNeed (Model.GetValue (iter, (int)comboDataColumns.Item));
			} else {
				SelectedItem = null;
			}
			base.OnChanged ();
		}

		protected object WrapValueIfNeed(object value)
		{
			if (value is string || value is int || value is uint || value is decimal || value is double || value is long || value is ulong || value is short || value is ushort)
				return new KeepInObject (value);
			else
				return value;
		}

		protected object UnWrapValueIfNeed(object value)
		{
			return value is KeepInObject ? ((KeepInObject)value).Value :  value;
		}

		//HACK workaround can not save to object column of ListStore simple types
		protected struct KeepInObject
		{
			public object Value;

			public KeepInObject(object value)
			{
				Value = value;
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


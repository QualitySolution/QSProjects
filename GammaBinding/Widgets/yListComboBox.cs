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

		bool notUserChanage = false;

		protected enum comboDataColumns {
			Title,
			Item
		}

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

		public Func<object, string> RenderTextFunc;

		public virtual void SetRenderTextFunc<TObject> (Func<TObject, string> renderTextFunc) where TObject : class
		{
			RenderTextFunc = o => renderTextFunc (o as TObject);
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
				if (value != null && !ListStoreHelper.SearchListStore (comboListStore, value, (int)comboDataColumns.Item, out iter))
					return;

				selectedItem = value;

				notUserChanage = true;
				if (value == null)
					Active = -1;
				else
					SetActiveIter (iter);
				notUserChanage = false;

				Binding.FireChange (				
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
			comboListStore.Clear ();

			if (ItemsList == null)
				return;

			foreach (var item in ItemsList) {
				comboListStore.AppendValues (
					RenderTextFunc == null ? item.ToString () : RenderTextFunc(item),
					item
				);
			}
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
				SelectedItem = Model.GetValue (iter, (int)comboDataColumns.Item);
			} else {
				SelectedItem = null;
			}
			base.OnChanged ();
		}
	}

}


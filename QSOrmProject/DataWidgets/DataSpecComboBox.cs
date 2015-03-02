using System;
using System.ComponentModel;
using System.Data.Bindings;
using System.Reflection;
using Gtk;
using NLog;
using Gtk.DataBindings;
using System.Collections;
using QSOrmProject;

namespace Gtk.DataBindings
{
	[ToolboxItem (true)]
	[Category ("QS Widgets")]
	public class DataSpecComboBox : ComboBox
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ListStore comboListStore;

		enum comboDataColumns {
			Title,
			Item
		}

		public event EventHandler<EnumItemClickedEventArgs> EnumItemSelected;

		object itemsDataSource;
		public object ItemsDataSource {
			get {
				return itemsDataSource;
			}
			set {
				if (itemsDataSource == value)
					return;
				itemsDataSource = value;

				ResetLayout ();
			}
		}

		string columnMappings;
		/// <summary>
		/// Link to Column Mappings in connected Adaptor 
		/// </summary>
		[Browsable (true), Category ("Data Binding"), Description ("Column Data mappings")]
		public string ColumnMappings { 
			get { return (columnMappings); }
			set { columnMappings = value; }
		}

		private object selectedItem;
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
			
		private bool showSpecialStateAll = false;
		[Browsable(true)]
		public bool ShowSpecialStateAll {
			get {
				return showSpecialStateAll;
			}
			set {
				showSpecialStateAll = value;
				ResetLayout ();
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
				ResetLayout ();
			}
		}

		public DataSpecComboBox () : base()
		{
			comboListStore = new ListStore (typeof(string), typeof(object));
			CellRendererText text = new CellRendererText ();
			Model = comboListStore;
			this.PackStart (text, false);
			this.AddAttribute (text, "text", (int)comboDataColumns.Title);
		}

		/// <summary>
		/// Обновляет данные виджета
		/// </summary>
		private void ResetLayout()
		{
			comboListStore.Clear ();

			if (ItemsDataSource == null)
				return;

			if (ItemsDataSource is IEnumerable == false)
				throw new NotSupportedException (string.Format("ItemsDataSource only supports IEnumerable types, specified was {0}", ItemsDataSource));

			//Заполняем специальные поля
			if(ShowSpecialStateAll)
			{
				AppendEnumItem (typeof(SpecialComboState).GetField ("All"));
			}
			if(ShowSpecialStateNot)
			{
				AppendEnumItem (typeof(SpecialComboState).GetField ("Not"));
			}

			//FIXME Временное решение нужно
			Adaptor tempAdaptor = new Adaptor ();
			MappedProperty mp = new MappedProperty (tempAdaptor, columnMappings);
			foreach (object subject in (ItemsDataSource as IEnumerable)) {
				tempAdaptor.Target = subject;
				comboListStore.AppendValues (mp.Value, subject);
			}

			if (ShowSpecialStateAll || ShowSpecialStateNot)
				Active = 0;
		}

		private void AppendEnumItem(FieldInfo info)
		{
			if (info.Name.Equals("value__"))
				return;
			string item = info.GetEnumTitle ();
			//string hint = info.GetEnumHint ();
			//p = (Gdk.Pixbuf) info.GetEnumIcon();
			//if (p != null)
			//	item.Image = new Gtk.Image (p);
			//if (!String.IsNullOrEmpty (hint))
			//	item.TooltipText = hint;
			comboListStore.AppendValues (item, info.GetValue (null));
		}

		void OnEnumItemSelected ()
		{
			if(EnumItemSelected != null)
			{
				EnumItemSelected (this, new EnumItemClickedEventArgs (SelectedItem));
			}
		}

		protected override void OnChanged ()
		{
			base.OnChanged ();
			TreeIter iter;

			if(this.GetActiveIter (out iter))
			{
				SelectedItem = Model.GetValue (iter, (int)comboDataColumns.Item);
			}
			else
			{
				SelectedItem = SpecialComboState.None;
			}
		}
	}
}


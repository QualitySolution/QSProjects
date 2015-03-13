using System;
using System.ComponentModel;
using System.Data.Bindings;
using System.Reflection;
using Gtk;
using NLog;
using Gtk.DataBindings;
using System.Collections;
using QSOrmProject;
using QSProjectsLib;

namespace Gtk.DataBindings
{
	[ToolboxItem (true)]
	[Category ("QS Widgets")]
	public class DataSpecComboBox : ComboBox, IAdaptableControl, IPostableControl
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ListStore comboListStore;
		private ControlAdaptor adaptor;

		enum comboDataColumns {
			Title,
			Item
		}

		public event EventHandler<EnumItemClickedEventArgs> ItemSelected;

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
				TreeIter iter;
				if (value == null)
				{
					this.Active = (ShowSpecialStateAll || ShowSpecialStateNot) ? 0 : -1;
					return;
				}
				if(ListStoreWorks.SearchListStore (comboListStore, value, 1, out iter ))
					SetActiveIter(iter);

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
			adaptor = new GtkControlAdaptor (this, false);
			adaptor.DisableMappingsDataTransfer = true;

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

			//FIXME Временное решение, нужно сделать через биндинг
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
			if(ItemSelected != null)
			{
				ItemSelected (this, new EnumItemClickedEventArgs (SelectedItem));
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

			adaptor.DemandInstantPost();
		}

		/// <summary>
		/// Resolves ControlAdaptor in read-only mode
		/// </summary>
		[Browsable (false), Category ("Data Binding")]
		public ControlAdaptor Adaptor {
			get { return (adaptor); }
		}

		/// <summary>
		/// Defines if DataSource is inherited fom parent controls or not
		/// </summary>
		[Category ("Data Binding"), Description ("Inherited Data Source")]
		public bool InheritedDataSource {
			get { return (adaptor.InheritedDataSource); }
			set { adaptor.InheritedDataSource = value; }
		}

		/// <summary>
		/// DataSource object control is connected to
		/// </summary>
		[Browsable (false), Category ("Data Binding")]
		public object DataSource {
			get { return (adaptor.DataSource); }
			set { adaptor.DataSource = value; }
		}

		/// <summary>
		/// Link to Mappings in connected Adaptor 
		/// </summary>
		[Category ("Data Binding"), Description ("Data mappings")]
		public string Mappings { 
			get { return (adaptor.Mappings); }
			set { adaptor.Mappings = value; }
		}

		/// <summary>
		/// Overrides basic Get data behaviour
		///
		/// Assigning this avoids any value transfer between object and data
		/// Basic assigning in DateCalendar for example is
		///    	Date = (DateTime) Adaptor.Value;
		/// where Date is the DateCalendar property and Adaptor.Value is direct
		/// reference to the mapped property
		///
		///     public delegate void UserGetDataEvent (ControlAdaptor Adaptor);
		/// </summary>
		public event CustomGetDataEvent CustomGetData {
			add { adaptor.CustomGetData += value; }
			remove { adaptor.CustomGetData -= value; }
		}

		/// <summary>
		/// Overrides basic Post data behaviour
		///
		/// Assigning this avoids any value transfer between object and data
		/// Basic assigning in DateCalendar for example is
		///    	adaptor.Value = Date;
		/// where Date is the DateCalendar property and Adaptor.Value is direct
		/// reference to the mapped property
		///
		///     public delegate void UserPostDataEvent (ControlAdaptor Adaptor);
		/// </summary>
		public event CustomPostDataEvent CustomPostData {
			add { adaptor.CustomPostData += value; }
			remove { adaptor.CustomPostData -= value; }
		}

		/// <summary>
		/// Overrides basic Get data behaviour
		///
		/// Assigning this avoids any value transfer between object and data
		/// Basic assigning in DateCalendar for example is
		///    	Date = (DateTime) Adaptor.Value;
		/// where Date is the DateCalendar property and Adaptor.Value is direct
		/// reference to the mapped property
		///
		///     public delegate void UserGetDataEvent (ControlAdaptor Adaptor);
		/// </summary>
		private Adaptor currentSelection = null;
		/// <summary>
		/// Allows controls to bind on the selection in this TreeView
		/// </summary>
		/// <remarks>
		/// DO NOT USE THIS ONE TO SET WHICH ITEM IS SELECTED.
		/// OR AT LEAST NOT YET.
		/// </remarks>
		[Browsable (false), Category ("Data Binding")]
		public Adaptor CurrentSelection {
			get { return (currentSelection); }
		}

		/// <summary>
		/// Calls ControlAdaptors method to transfer data, so it can be wrapped
		/// into widget specific things and all checkups
		/// </summary>
		/// <param name="aSender">
		/// Sender object <see cref="System.Object"/>
		/// </param>
		public void CallAdaptorGetData (object aSender)
		{
			Adaptor.InvokeAdapteeDataChange (this, aSender);
		}

		/// <summary>
		/// Notification method activated from Adaptor 
		/// </summary>
		/// <param name="aSender">
		/// Object that made change <see cref="System.Object"/>
		/// </param>
		public virtual void GetDataFromDataSource (object aSender)
		{
			adaptor.DataChanged = false;
			if (Model.IterNChildren () == 0)
				return;
			TreeIter iter;
			if (adaptor.Value == null)
			{
				this.Active = (ShowSpecialStateAll || ShowSpecialStateNot) ? 0 : -1;
			} else if(ListStoreWorks.SearchListStore (comboListStore, adaptor.Value, 1, out iter ))
				SetActiveIter(iter);
		}

		/// <summary>
		/// Updates parent object to DataSource object
		/// </summary>
		public virtual void PutDataToDataSource (object aSender)
		{
			adaptor.DataChanged = false;
			if (SelectedItem is SpecialComboState)
				adaptor.Value = null;
			else
				adaptor.Value = SelectedItem;
		}

		/// <summary>
		/// Defines if DataSource is inherited fom parent controls or not
		/// </summary>
		[Category ("Data Binding"), Description ("Inherited Data Source")]
		public bool InheritedBoundaryDataSource {
			get { return (adaptor.InheritedBoundaryDataSource); }
			set { adaptor.InheritedBoundaryDataSource = value; }
		}

		/// <summary>
		/// DataSource object control is connected to
		/// </summary>
		[Browsable (false), Category ("Data Binding")]
		public IObserveable BoundaryDataSource {
			get { return (adaptor.BoundaryDataSource); }
			set { adaptor.BoundaryDataSource = value; }
		}

		/// <summary>
		/// Link to Mappings in connected Adaptor 
		/// </summary>
		[Category ("Data Binding"), Description ("Data Mappings")]
		public string BoundaryMappings { 
			get { return (adaptor.BoundaryMappings); }
			set { adaptor.BoundaryMappings = value; }
		}

	}
}


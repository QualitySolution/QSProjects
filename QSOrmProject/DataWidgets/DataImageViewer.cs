using System;
using System.ComponentModel;
using System.Data.Bindings;
using Gtk.DataBindings;
using QSWidgetLib;

namespace QSOrmProject
{
	/// <summary>
	/// Image control connected to adaptor and direct updating if connected object
	/// supports IChangeable
	///
	/// Supports single mapping
	/// </summary>
	[ToolboxItem (true)]
	[Category ("Databound Widgets")]
	//[GtkWidgetFactoryProvider ("pixbuf", "DefaultFactoryCreate")]
	//[GtkWidgetFactoryProvider ("picture", "DefaultFactoryCreate")]
	//[GtkTypeWidgetFactoryProvider ("picturehandler", "DefaultFactoryCreate", typeof(Gdk.Pixbuf))]
	public class DataImageViewer : ImageViewer, IAdaptableControl, IPostableControl
	{
		/// <summary>
		/// Registered factory creation method
		/// </summary>
		/// <param name="aArgs">
		/// Arguments <see cref="FactoryInvocationArgs"/>
		/// </param>
		/// <returns>
		/// Result widget <see cref="IAdaptableControl"/>
		/// </returns>
		public static IAdaptableControl DefaultFactoryCreate (FactoryInvocationArgs aArgs)
		{
			IAdaptableControl wdg;
			//			if (aArgs.State == PropertyDefinition.ReadOnly)
			wdg = new DataImage();
			//			else
			//				wdg = new DataEntry();
			wdg.Mappings = aArgs.PropertyName;
			return (wdg);
		}

		private ControlAdaptor adaptor = null;

		/// <summary>
		/// Resolves ControlAdaptor in read-only mode
		/// </summary>
		[Browsable (false), Category ("Data Binding")]
		public ControlAdaptor Adaptor {
			get { return (adaptor); }
		}

		/// <summary>
		/// Defines if BoundaryDataSource is inherited fom parent controls or not
		/// </summary>
		[Category ("Data Binding"), Description ("Inherited Data Source")]
		public bool InheritedBoundaryDataSource {
			get { return (adaptor.InheritedBoundaryDataSource); }
			set { adaptor.InheritedBoundaryDataSource = value; }
		}

		/// <summary>
		/// BoundaryDataSource object control is connected to
		/// </summary>
		[Browsable (false), Category ("Data Binding")]
		public IObserveable BoundaryDataSource {
			get { return (adaptor.BoundaryDataSource); }
			set { adaptor.BoundaryDataSource = value; }
		}

		/// <summary>
		/// Link to BoundaryMappings in connected Adaptor 
		/// </summary>
		[Category ("Data Binding"), Description ("Data Mappings")]
		public string BoundaryMappings { 
			get { return (adaptor.BoundaryMappings); }
			set { adaptor.BoundaryMappings = value; }
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
		/// You can map Gtk.Pixbuf or byte[] value
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
			// Translate enumeration value
			// FIXME Возможно не надо.
			/*if ((adaptor.Adaptor.FinalTarget != null) && (Mappings != "")) {
				System.Type type = adaptor.Adaptor.Values[0].Value.GetType();
				if (type.IsEnum == true) {
					Array enumValues = Enum.GetValues (type);
					for (int i=0; i<enumValues.Length; i++)
						if (enumValues.GetValue(i).Equals(adaptor.Value)) {
							string desc = enumValues.GetValue(i).ToString();
							//System.Console.WriteLine(enumValues.GetValue(i).GetType().GetField(desc).GetEnumIcon().GetType());
							pixbuf = (Gdk.Pixbuf) enumValues.GetValue(i).GetType().GetField(desc).GetEnumIcon();
						}
					return;
				}
			}*/
			if (adaptor.Adaptor.HasDefaultMapping == true)
			{
				if(adaptor.Value is Gdk.Pixbuf)
					pixbuf = (Gdk.Pixbuf) adaptor.Value;
				if(adaptor.Value is byte[])
				{
					pixbuf = new Gdk.Pixbuf(adaptor.Value as byte[]);
				}
			}
				
		}

		/// <summary>
		/// Updates parent object to DataSource object
		/// </summary>
		public virtual void PutDataToDataSource (object aSender)
		{
			adaptor.DataChanged = false;
			if (adaptor.Adaptor.HasDefaultMapping == true)
				adaptor.Value = pixbuf;
		}

		/// <summary>
		/// Creates Widget 
		/// </summary>
		public DataImageViewer()
			: base ()
		{
			adaptor = new GtkControlAdaptor (this, true);
		}

		/// <summary>
		/// Creates Widget 
		/// </summary>
		/// <param name="aMappings">
		/// Mappings with this widget <see cref="System.String"/>
		/// </param>
		public DataImageViewer (string aMappings)
			: base ()
		{
			adaptor = new GtkControlAdaptor (this, true, aMappings);
		}

		/// <summary>
		/// Creates Widget 
		/// </summary>
		/// <param name="aDataSource">
		/// DataSource connected to this widget <see cref="System.Object"/>
		/// </param>
		/// <param name="aMappings">
		/// Mappings with this widget <see cref="System.String"/>
		/// </param>
		public DataImageViewer (object aDataSource, string aMappings)
			: base ()
		{
			adaptor = new GtkControlAdaptor (this, true, aDataSource, aMappings);
		}

		/// <summary>
		/// Destroys and disconnects Widget
		/// </summary>
		~DataImageViewer()
		{
			adaptor.Disconnect();
			adaptor = null;
		}
	}

}



// This file has been generated by the GUI designer. Do not modify.
namespace QSProjectsLib
{
	public partial class Reference
	{
		private global::Gtk.UIManager UIManager;
		private global::Gtk.Action addAction;
		private global::Gtk.Action editAction;
		private global::Gtk.Action removeAction;
		private global::Gtk.Action addAction1;
		private global::Gtk.Action editAction1;
		private global::Gtk.Action removeAction1;
		private global::Gtk.VBox vbox2;
		private global::Gtk.Toolbar toolbar1;
		private global::Gtk.HBox hbox1;
		private global::Gtk.Label label1;
		private global::Gtk.Entry entryFilter;
		private global::Gtk.Button buttonClean;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TreeView treeviewref;
		private global::Gtk.Button buttonClose;
		private global::Gtk.Button buttonCancel;
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget QSProjectsLib.Reference
			this.UIManager = new global::Gtk.UIManager ();
			global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
			this.addAction = new global::Gtk.Action ("addAction", global::Mono.Unix.Catalog.GetString ("Добавить"), null, "gtk-add");
			this.addAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Добавить");
			w1.Add (this.addAction, null);
			this.editAction = new global::Gtk.Action ("editAction", null, null, "gtk-edit");
			w1.Add (this.editAction, null);
			this.removeAction = new global::Gtk.Action ("removeAction", null, null, "gtk-remove");
			w1.Add (this.removeAction, null);
			this.addAction1 = new global::Gtk.Action ("addAction1", null, null, "gtk-add");
			this.addAction1.ShortLabel = global::Mono.Unix.Catalog.GetString ("Добавить");
			w1.Add (this.addAction1, null);
			this.editAction1 = new global::Gtk.Action ("editAction1", null, null, "gtk-edit");
			this.editAction1.ShortLabel = global::Mono.Unix.Catalog.GetString ("Изменить");
			w1.Add (this.editAction1, null);
			this.removeAction1 = new global::Gtk.Action ("removeAction1", null, null, "gtk-remove");
			this.removeAction1.ShortLabel = global::Mono.Unix.Catalog.GetString ("Удалить");
			w1.Add (this.removeAction1, null);
			this.UIManager.InsertActionGroup (w1, 0);
			this.AddAccelGroup (this.UIManager.AccelGroup);
			this.Name = "QSProjectsLib.Reference";
			this.Title = global::Mono.Unix.Catalog.GetString ("Справочник");
			this.Icon = global::Stetic.IconLoader.LoadIcon (this, "stock_addressbook", global::Gtk.IconSize.LargeToolbar);
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child QSProjectsLib.Reference.VBox
			global::Gtk.VBox w2 = this.VBox;
			w2.Name = "dialog1_VBox";
			w2.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.UIManager.AddUiFromString ("<ui><toolbar name='toolbar1'><toolitem name='addAction1' action='addAction1'/><toolitem name='editAction1' action='editAction1'/><toolitem name='removeAction1' action='removeAction1'/></toolbar></ui>");
			this.toolbar1 = ((global::Gtk.Toolbar)(this.UIManager.GetWidget ("/toolbar1")));
			this.toolbar1.Name = "toolbar1";
			this.toolbar1.ShowArrow = false;
			this.toolbar1.ToolbarStyle = ((global::Gtk.ToolbarStyle)(2));
			this.toolbar1.IconSize = ((global::Gtk.IconSize)(3));
			this.vbox2.Add (this.toolbar1);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.toolbar1]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Быстрый поиск по имени:");
			this.hbox1.Add (this.label1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.label1]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.entryFilter = new global::Gtk.Entry ();
			this.entryFilter.CanFocus = true;
			this.entryFilter.Name = "entryFilter";
			this.entryFilter.IsEditable = true;
			this.entryFilter.InvisibleChar = '●';
			this.hbox1.Add (this.entryFilter);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.entryFilter]));
			w5.Position = 1;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonClean = new global::Gtk.Button ();
			this.buttonClean.TooltipMarkup = "Очистить";
			this.buttonClean.CanFocus = true;
			this.buttonClean.Name = "buttonClean";
			this.buttonClean.UseUnderline = true;
			// Container child buttonClean.Gtk.Container+ContainerChild
			global::Gtk.Alignment w6 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w7 = new global::Gtk.HBox ();
			w7.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w8 = new global::Gtk.Image ();
			w8.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-clear", global::Gtk.IconSize.Menu);
			w7.Add (w8);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w10 = new global::Gtk.Label ();
			w7.Add (w10);
			w6.Add (w7);
			this.buttonClean.Add (w6);
			this.hbox1.Add (this.buttonClean);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.buttonClean]));
			w14.Position = 2;
			w14.Expand = false;
			w14.Fill = false;
			this.vbox2.Add (this.hbox1);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox1]));
			w15.Position = 1;
			w15.Expand = false;
			w15.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeviewref = new global::Gtk.TreeView ();
			this.treeviewref.CanFocus = true;
			this.treeviewref.Name = "treeviewref";
			this.GtkScrolledWindow.Add (this.treeviewref);
			this.vbox2.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.GtkScrolledWindow]));
			w17.Position = 2;
			w2.Add (this.vbox2);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(w2 [this.vbox2]));
			w18.Position = 0;
			// Internal child QSProjectsLib.Reference.ActionArea
			global::Gtk.HButtonBox w19 = this.ActionArea;
			w19.Name = "dialog1_ActionArea";
			w19.Spacing = 10;
			w19.BorderWidth = ((uint)(5));
			w19.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonClose = new global::Gtk.Button ();
			this.buttonClose.CanFocus = true;
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.UseUnderline = true;
			// Container child buttonClose.Gtk.Container+ContainerChild
			global::Gtk.Alignment w20 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w21 = new global::Gtk.HBox ();
			w21.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w22 = new global::Gtk.Image ();
			w22.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-close", global::Gtk.IconSize.Menu);
			w21.Add (w22);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w24 = new global::Gtk.Label ();
			w24.LabelProp = global::Mono.Unix.Catalog.GetString ("_Закрыть");
			w24.UseUnderline = true;
			w21.Add (w24);
			w20.Add (w21);
			this.buttonClose.Add (w20);
			this.AddActionWidget (this.buttonClose, -7);
			global::Gtk.ButtonBox.ButtonBoxChild w28 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w19 [this.buttonClose]));
			w28.Expand = false;
			w28.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			// Container child buttonCancel.Gtk.Container+ContainerChild
			global::Gtk.Alignment w29 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w30 = new global::Gtk.HBox ();
			w30.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w31 = new global::Gtk.Image ();
			w31.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-cancel", global::Gtk.IconSize.Menu);
			w30.Add (w31);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w33 = new global::Gtk.Label ();
			w33.LabelProp = global::Mono.Unix.Catalog.GetString ("О_тменить");
			w33.UseUnderline = true;
			w30.Add (w33);
			w29.Add (w30);
			this.buttonCancel.Add (w29);
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w37 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w19 [this.buttonCancel]));
			w37.Position = 1;
			w37.Expand = false;
			w37.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.Sensitive = false;
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseUnderline = true;
			// Container child buttonOk.Gtk.Container+ContainerChild
			global::Gtk.Alignment w38 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w39 = new global::Gtk.HBox ();
			w39.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w40 = new global::Gtk.Image ();
			w40.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-ok", global::Gtk.IconSize.Menu);
			w39.Add (w40);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w42 = new global::Gtk.Label ();
			w42.LabelProp = global::Mono.Unix.Catalog.GetString ("_OK");
			w42.UseUnderline = true;
			w39.Add (w42);
			w38.Add (w39);
			this.buttonOk.Add (w38);
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w46 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w19 [this.buttonOk]));
			w46.Position = 2;
			w46.Expand = false;
			w46.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 482;
			this.DefaultHeight = 450;
			this.Show ();
			this.addAction.Activated += new global::System.EventHandler (this.OnAddActionActivated);
			this.editAction.Activated += new global::System.EventHandler (this.OnEditActionActivated);
			this.removeAction.Activated += new global::System.EventHandler (this.OnRemoveActionActivated);
			this.addAction1.Activated += new global::System.EventHandler (this.OnAddActionActivated);
			this.editAction1.Activated += new global::System.EventHandler (this.OnEditActionActivated);
			this.removeAction1.Activated += new global::System.EventHandler (this.OnRemoveActionActivated);
			this.entryFilter.Changed += new global::System.EventHandler (this.OnEntryFilterChanged);
			this.buttonClean.Clicked += new global::System.EventHandler (this.OnButtonCleanClicked);
			this.treeviewref.CursorChanged += new global::System.EventHandler (this.OnTreeviewrefCursorChanged);
			this.treeviewref.RowActivated += new global::Gtk.RowActivatedHandler (this.OnTreeviewrefRowActivated);
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
		}
	}
}


// This file has been generated by the GUI designer. Do not modify.
namespace QSChat
{
	public partial class ChatHistory
	{
		private global::Gtk.VBox vbox2;
		
		private global::Gtk.HBox hbox2;
		
		private global::Gtk.ScrolledWindow GtkScrolledWindow1;
		
		private global::Gtk.TreeView treeviewDates;
		
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		
		private global::Gtk.TextView textviewTalks;
		
		private global::Gtk.HButtonBox hbuttonbox2;
		
		private global::Gtk.Button buttonClose;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget QSChat.ChatHistory
			this.Name = "QSChat.ChatHistory";
			this.Title = global::Mono.Unix.Catalog.GetString ("Архив чата");
			this.Icon = global::Gdk.Pixbuf.LoadFromResource ("QSChat.icons.document-open-recent.png");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Container child QSChat.ChatHistory.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.treeviewDates = new global::Gtk.TreeView ();
			this.treeviewDates.CanFocus = true;
			this.treeviewDates.Name = "treeviewDates";
			this.GtkScrolledWindow1.Add (this.treeviewDates);
			this.hbox2.Add (this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.GtkScrolledWindow1]));
			w2.Position = 0;
			w2.Expand = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.textviewTalks = new global::Gtk.TextView ();
			this.textviewTalks.CanFocus = true;
			this.textviewTalks.Name = "textviewTalks";
			this.textviewTalks.Editable = false;
			this.textviewTalks.WrapMode = ((global::Gtk.WrapMode)(3));
			this.GtkScrolledWindow.Add (this.textviewTalks);
			this.hbox2.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.GtkScrolledWindow]));
			w4.Position = 1;
			this.vbox2.Add (this.hbox2);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox2]));
			w5.Position = 0;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbuttonbox2 = new global::Gtk.HButtonBox ();
			this.hbuttonbox2.Name = "hbuttonbox2";
			this.hbuttonbox2.BorderWidth = ((uint)(3));
			this.hbuttonbox2.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child hbuttonbox2.Gtk.ButtonBox+ButtonBoxChild
			this.buttonClose = new global::Gtk.Button ();
			this.buttonClose.CanFocus = true;
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.UseStock = true;
			this.buttonClose.UseUnderline = true;
			this.buttonClose.Label = "gtk-close";
			this.hbuttonbox2.Add (this.buttonClose);
			global::Gtk.ButtonBox.ButtonBoxChild w6 = ((global::Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox2 [this.buttonClose]));
			w6.Expand = false;
			w6.Fill = false;
			this.vbox2.Add (this.hbuttonbox2);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbuttonbox2]));
			w7.Position = 1;
			w7.Expand = false;
			this.Add (this.vbox2);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 546;
			this.DefaultHeight = 348;
			this.Show ();
			this.buttonClose.Clicked += new global::System.EventHandler (this.OnButtonCloseClicked);
		}
	}
}

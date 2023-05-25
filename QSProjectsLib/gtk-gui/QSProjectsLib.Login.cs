
// This file has been generated by the GUI designer. Do not modify.
namespace QSProjectsLib
{
	public partial class Login
	{
		private global::Gtk.VBox vbox2;
		
		private global::Gtk.Image imageLogo;
		
		private global::Gtk.Label labelAppName;
		
		private global::Gtk.Table table1;
		
		private global::Gtk.Button buttonDemo;
		
		private global::Gtk.Button buttonEditConnection;
		
		private global::Gtk.ComboBox comboboxConnections;
		
		private global::Gtk.Entry entryPassword;
		
		private global::Gtk.Entry entryUser;
		
		private global::Gtk.Label label1;
		
		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gtk.Label label4;

		private global::Gtk.Label label5;

		private global::Gtk.Label labelKeyboardLayoutInfo;

		private global::Gtk.Label labelCapslockInfo;

		private global::Gtk.HBox hbox1;
		
		private global::Gtk.Label labelLoginInfo;
		
		private global::Gtk.Button buttonErrorInfo;
		
		private global::Gtk.Button buttonCancel;
		
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget QSProjectsLib.Login
			this.Name = "QSProjectsLib.Login";
			this.Title = global::Mono.Unix.Catalog.GetString ("Вход в систему");
			this.Icon = global::Stetic.IconLoader.LoadIcon (this, "gtk-connect", global::Gtk.IconSize.LargeToolbar);
			this.WindowPosition = ((global::Gtk.WindowPosition)(3));
			// Internal child QSProjectsLib.Login.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.imageLogo = new global::Gtk.Image ();
			this.imageLogo.WidthRequest = 0;
			this.imageLogo.HeightRequest = 0;
			this.imageLogo.Name = "imageLogo";
			this.vbox2.Add (this.imageLogo);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.imageLogo]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.labelAppName = new global::Gtk.Label ();
			this.labelAppName.Name = "labelAppName";
			this.labelAppName.Xpad = 25;
			this.labelAppName.Xalign = 1F;
			this.labelAppName.LabelProp = global::Mono.Unix.Catalog.GetString ("<span foreground=\"gray\" size=\"large\" font_family=\"Philosopher\"><b>QS: Платформа v.1.0</b></span>");
			this.labelAppName.UseMarkup = true;
			this.labelAppName.Justify = ((global::Gtk.Justification)(1));
			this.vbox2.Add (this.labelAppName);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.labelAppName]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(3)), ((uint)(5)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			this.table1.BorderWidth = ((uint)(18));
			// Container child table1.Gtk.Table+TableChild
			this.buttonDemo = new global::Gtk.Button ();
			this.buttonDemo.CanFocus = true;
			this.buttonDemo.Name = "buttonDemo";
			this.buttonDemo.UseUnderline = true;
			this.buttonDemo.Relief = ((global::Gtk.ReliefStyle)(2));
			global::Gtk.Image w4 = new global::Gtk.Image ();
			w4.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-dialog-info", global::Gtk.IconSize.Button);
			this.buttonDemo.Image = w4;
			this.table1.Add (this.buttonDemo);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1 [this.buttonDemo]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(3));
			w5.RightAttach = ((uint)(4));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.buttonEditConnection = new global::Gtk.Button ();
			this.buttonEditConnection.TooltipMarkup = "Редактор подключений";
			this.buttonEditConnection.CanFocus = true;
			this.buttonEditConnection.Name = "buttonEditConnection";
			this.buttonEditConnection.UseUnderline = true;
			this.buttonEditConnection.Relief = ((global::Gtk.ReliefStyle)(2));
			global::Gtk.Image w6 = new global::Gtk.Image ();
			w6.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-edit", global::Gtk.IconSize.Menu);
			this.buttonEditConnection.Image = w6;
			this.table1.Add (this.buttonEditConnection);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1 [this.buttonEditConnection]));
			w7.LeftAttach = ((uint)(3));
			w7.RightAttach = ((uint)(4));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.comboboxConnections = global::Gtk.ComboBox.NewText ();
			this.comboboxConnections.Name = "comboboxConnections";
			this.table1.Add (this.comboboxConnections);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1 [this.comboboxConnections]));
			w8.LeftAttach = ((uint)(2));
			w8.RightAttach = ((uint)(3));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryPassword = new global::Gtk.Entry ();
			this.entryPassword.CanFocus = true;
			this.entryPassword.Name = "entryPassword";
			this.entryPassword.IsEditable = true;
			this.entryPassword.Visibility = false;
			this.entryPassword.InvisibleChar = '●';
			this.table1.Add (this.entryPassword);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryPassword]));
			w9.TopAttach = ((uint)(2));
			w9.BottomAttach = ((uint)(3));
			w9.LeftAttach = ((uint)(2));
			w9.RightAttach = ((uint)(3));
			w9.XOptions = ((global::Gtk.AttachOptions)(4));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryUser = new global::Gtk.Entry ();
			this.entryUser.CanFocus = true;
			this.entryUser.Name = "entryUser";
			this.entryUser.IsEditable = true;
			this.entryUser.InvisibleChar = '●';
			this.table1.Add (this.entryUser);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryUser]));
			w10.TopAttach = ((uint)(1));
			w10.BottomAttach = ((uint)(2));
			w10.LeftAttach = ((uint)(2));
			w10.RightAttach = ((uint)(3));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Пользователь:");
			this.label1.Justify = ((global::Gtk.Justification)(1));
			this.table1.Add (this.label1);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1 [this.label1]));
			w11.TopAttach = ((uint)(1));
			w11.BottomAttach = ((uint)(2));
			w11.LeftAttach = ((uint)(1));
			w11.RightAttach = ((uint)(2));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Пароль:");
			this.label2.Justify = ((global::Gtk.Justification)(1));
			this.table1.Add (this.label2);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1 [this.label2]));
			w12.TopAttach = ((uint)(2));
			w12.BottomAttach = ((uint)(3));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(2));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("Подключение:");
			this.label4.Justify = ((global::Gtk.Justification)(1));
			this.table1.Add(this.label4);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1[this.label4]));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.table1.Add(this.label5);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.label5]));
			w15.LeftAttach = ((uint)(4));
			w15.RightAttach = ((uint)(5));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.table1);
			global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.table1]));
			w16.Position = 2;
			w16.Expand = false;
			w16.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.labelKeyboardLayoutInfo = new global::Gtk.Label();
			this.labelKeyboardLayoutInfo.Name = "labelKeyboardLayoutInfo";
			this.labelKeyboardLayoutInfo.LabelProp = global::Mono.Unix.Catalog.GetString("<span foreground=\"red\"><b>Внимание: включен русский язык</b></span>");
			this.labelKeyboardLayoutInfo.UseMarkup = true;
			this.vbox2.Add(this.labelKeyboardLayoutInfo);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.labelKeyboardLayoutInfo]));
			w17.Position = 3;
			w17.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.labelCapslockInfo = new global::Gtk.Label();
			this.labelCapslockInfo.Name = "labelCapslockInfo";
			this.labelCapslockInfo.LabelProp = global::Mono.Unix.Catalog.GetString("<span foreground=\"red\"><b>Внимание: включен Caps Lock</b></span>");
			this.labelCapslockInfo.UseMarkup = true;
			this.vbox2.Add(this.labelCapslockInfo);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.labelCapslockInfo]));
			w18.Position = 4;
			w18.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			this.hbox1.BorderWidth = ((uint)(6));
			// Container child hbox1.Gtk.Box+BoxChild
			this.labelLoginInfo = new global::Gtk.Label();
			this.labelLoginInfo.Name = "labelLoginInfo";
			this.labelLoginInfo.Wrap = true;
			this.hbox1.Add(this.labelLoginInfo);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.labelLoginInfo]));
			w19.Position = 0;
			w19.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonErrorInfo = new global::Gtk.Button();
			this.buttonErrorInfo.CanFocus = true;
			this.buttonErrorInfo.Name = "buttonErrorInfo";
			this.buttonErrorInfo.UseUnderline = true;
			this.buttonErrorInfo.Label = global::Mono.Unix.Catalog.GetString("Подробнее..");
			global::Gtk.Image w20 = new global::Gtk.Image();
			w20.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-dialog-error", global::Gtk.IconSize.Button);
			this.buttonErrorInfo.Image = w20;
			this.hbox1.Add(this.buttonErrorInfo);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonErrorInfo]));
			w21.Position = 1;
			w21.Fill = false;
			this.vbox2.Add(this.hbox1);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox1]));
			w22.Position = 5;
			w22.Expand = false;
			w22.Fill = false;
			w1.Add(this.vbox2);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(w1[this.vbox2]));
			w23.Position = 0;
			w23.Expand = false;
			w23.Fill = false;
			// Internal child QSProjectsLib.Login.ActionArea
			global::Gtk.HButtonBox w24 = this.ActionArea;
			w24.Name = "dialog1_ActionArea";
			w24.Spacing = 10;
			w24.BorderWidth = ((uint)(5));
			w24.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("О_тменить");
			global::Gtk.Image w25 = new global::Gtk.Image();
			w25.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-cancel", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w25;
			this.AddActionWidget(this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w26 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w24[this.buttonCancel]));
			w26.Expand = false;
			w26.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = global::Mono.Unix.Catalog.GetString("_OK");
			global::Gtk.Image w27 = new global::Gtk.Image();
			w27.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-ok", global::Gtk.IconSize.Menu);
			this.buttonOk.Image = w27;
			w24.Add(this.buttonOk);
			global::Gtk.ButtonBox.ButtonBoxChild w28 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w24[this.buttonOk]));
			w28.Position = 1;
			w28.Expand = false;
			w28.Fill = false;
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.DefaultWidth = 355;
			this.DefaultHeight = 358;
			this.buttonDemo.Hide ();
			this.labelKeyboardLayoutInfo.Hide();
			this.labelCapslockInfo.Hide();
			this.buttonErrorInfo.Hide();
			this.Show();
			this.entryUser.Activated += new global::System.EventHandler (this.OnEntryActivated);
			this.entryPassword.Activated += new global::System.EventHandler (this.OnEntryPasswordActivated);
			this.comboboxConnections.Changed += new global::System.EventHandler (this.OnComboboxConnectionsChanged);
			this.buttonEditConnection.Clicked += new global::System.EventHandler (this.OnButtonEditConnectionClicked);
			this.buttonDemo.Clicked += new global::System.EventHandler (this.OnButtonDemoClicked);
			this.buttonErrorInfo.Clicked += new global::System.EventHandler (this.OnButtonErrorInfoClicked);
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
		}
	}
}

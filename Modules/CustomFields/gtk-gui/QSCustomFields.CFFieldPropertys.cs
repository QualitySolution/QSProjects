
// This file has been generated by the GUI designer. Do not modify.
namespace QSCustomFields
{
	public partial class CFFieldPropertys
	{
		private global::Gtk.Table table1;
		
		private global::Gtk.ComboBox comboDataType;
		
		private global::Gtk.ComboBox comboFieldType;
		
		private global::Gtk.Entry entryDBName;
		
		private global::Gtk.Entry entryID;
		
		private global::Gtk.Entry entryName;
		
		private global::Gtk.HBox hbox1;
		
		private global::Gtk.SpinButton spinSize;
		
		private global::Gtk.Label labelSizeSeparator;
		
		private global::Gtk.SpinButton spinDigits;
		
		private global::Gtk.Label label1;
		
		private global::Gtk.Label label2;
		
		private global::Gtk.Label label3;
		
		private global::Gtk.Label label4;
		
		private global::Gtk.Label label5;
		
		private global::Gtk.Label label6;
		
		private global::Gtk.Label label7;
		
		private global::Gtk.Label label8;
		
		private global::Gtk.VBox vbox2;
		
		private global::Gtk.CheckButton checkbuttonDisplay;
		
		private global::Gtk.CheckButton checkbuttonSearch;
		
		private global::Gtk.Button buttonCancel;
		
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget QSCustomFields.CFFieldPropertys
			this.Name = "QSCustomFields.CFFieldPropertys";
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child QSCustomFields.CFFieldPropertys.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(9)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			this.table1.BorderWidth = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.comboDataType = global::Gtk.ComboBox.NewText ();
			this.comboDataType.AppendText (global::Mono.Unix.Catalog.GetString ("VARCHAR"));
			this.comboDataType.AppendText (global::Mono.Unix.Catalog.GetString ("DECIMAL"));
			this.comboDataType.Sensitive = false;
			this.comboDataType.Name = "comboDataType";
			this.comboDataType.Active = 0;
			this.table1.Add (this.comboDataType);
			global::Gtk.Table.TableChild w2 = ((global::Gtk.Table.TableChild)(this.table1 [this.comboDataType]));
			w2.TopAttach = ((uint)(6));
			w2.BottomAttach = ((uint)(7));
			w2.LeftAttach = ((uint)(1));
			w2.RightAttach = ((uint)(2));
			w2.XOptions = ((global::Gtk.AttachOptions)(4));
			w2.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.comboFieldType = global::Gtk.ComboBox.NewText ();
			this.comboFieldType.AppendText (global::Mono.Unix.Catalog.GetString ("Строка"));
			this.comboFieldType.AppendText (global::Mono.Unix.Catalog.GetString ("Деньги"));
			this.comboFieldType.Name = "comboFieldType";
			this.comboFieldType.Active = 0;
			this.table1.Add (this.comboFieldType);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1 [this.comboFieldType]));
			w3.TopAttach = ((uint)(2));
			w3.BottomAttach = ((uint)(3));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryDBName = new global::Gtk.Entry ();
			this.entryDBName.CanFocus = true;
			this.entryDBName.Name = "entryDBName";
			this.entryDBName.IsEditable = true;
			this.entryDBName.InvisibleChar = '●';
			this.table1.Add (this.entryDBName);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryDBName]));
			w4.TopAttach = ((uint)(5));
			w4.BottomAttach = ((uint)(6));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryID = new global::Gtk.Entry ();
			this.entryID.Sensitive = false;
			this.entryID.CanFocus = true;
			this.entryID.Name = "entryID";
			this.entryID.IsEditable = true;
			this.entryID.InvisibleChar = '●';
			this.table1.Add (this.entryID);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryID]));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryName = new global::Gtk.Entry ();
			this.entryName.CanFocus = true;
			this.entryName.Name = "entryName";
			this.entryName.IsEditable = true;
			this.entryName.InvisibleChar = '●';
			this.table1.Add (this.entryName);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryName]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.LeftAttach = ((uint)(1));
			w6.RightAttach = ((uint)(2));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.spinSize = new global::Gtk.SpinButton (1, 255, 1);
			this.spinSize.CanFocus = true;
			this.spinSize.Name = "spinSize";
			this.spinSize.Adjustment.PageIncrement = 10;
			this.spinSize.ClimbRate = 1;
			this.spinSize.Numeric = true;
			this.spinSize.Value = 15;
			this.hbox1.Add (this.spinSize);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.spinSize]));
			w7.Position = 0;
			// Container child hbox1.Gtk.Box+BoxChild
			this.labelSizeSeparator = new global::Gtk.Label ();
			this.labelSizeSeparator.Name = "labelSizeSeparator";
			this.labelSizeSeparator.LabelProp = global::Mono.Unix.Catalog.GetString (",");
			this.hbox1.Add (this.labelSizeSeparator);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.labelSizeSeparator]));
			w8.Position = 1;
			w8.Expand = false;
			w8.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.spinDigits = new global::Gtk.SpinButton (0, 30, 1);
			this.spinDigits.TooltipMarkup = "Количество знаков после запятой";
			this.spinDigits.CanFocus = true;
			this.spinDigits.Name = "spinDigits";
			this.spinDigits.Adjustment.PageIncrement = 10;
			this.spinDigits.ClimbRate = 1;
			this.spinDigits.Numeric = true;
			this.spinDigits.Value = 2;
			this.hbox1.Add (this.spinDigits);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.spinDigits]));
			w9.Position = 2;
			this.table1.Add (this.hbox1);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1 [this.hbox1]));
			w10.TopAttach = ((uint)(7));
			w10.BottomAttach = ((uint)(8));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Тип поля<span foreground=\"red\">*</span>:");
			this.label1.UseMarkup = true;
			this.table1.Add (this.label1);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1 [this.label1]));
			w11.TopAttach = ((uint)(2));
			w11.BottomAttach = ((uint)(3));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Код:");
			this.table1.Add (this.label2);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1 [this.label2]));
			w12.XOptions = ((global::Gtk.AttachOptions)(4));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label ();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString ("Имя поля<span foreground=\"red\">*</span>:");
			this.label3.UseMarkup = true;
			this.table1.Add (this.label3);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.table1 [this.label3]));
			w13.TopAttach = ((uint)(1));
			w13.BottomAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label4 = new global::Gtk.Label ();
			this.label4.Name = "label4";
			this.label4.Xalign = 1F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString ("Имя в БД<span foreground=\"red\">*</span>:");
			this.label4.UseMarkup = true;
			this.table1.Add (this.label4);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.table1 [this.label4]));
			w14.TopAttach = ((uint)(5));
			w14.BottomAttach = ((uint)(6));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label5 = new global::Gtk.Label ();
			this.label5.Sensitive = false;
			this.label5.Name = "label5";
			this.label5.Xalign = 1F;
			this.label5.Yalign = 0F;
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString ("Свойства:");
			this.table1.Add (this.label5);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1 [this.label5]));
			w15.TopAttach = ((uint)(3));
			w15.BottomAttach = ((uint)(4));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label ();
			this.label6.Name = "label6";
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString ("<b>Настройки хранения в БД</b>");
			this.label6.UseMarkup = true;
			this.table1.Add (this.label6);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1 [this.label6]));
			w16.TopAttach = ((uint)(4));
			w16.BottomAttach = ((uint)(5));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label ();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString ("Тип данных<span foreground=\"red\">*</span>:");
			this.label7.UseMarkup = true;
			this.table1.Add (this.label7);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1 [this.label7]));
			w17.TopAttach = ((uint)(6));
			w17.BottomAttach = ((uint)(7));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label8 = new global::Gtk.Label ();
			this.label8.Name = "label8";
			this.label8.Xalign = 1F;
			this.label8.LabelProp = global::Mono.Unix.Catalog.GetString ("Размер<span foreground=\"red\">*</span>:");
			this.label8.UseMarkup = true;
			this.table1.Add (this.label8);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table1 [this.label8]));
			w18.TopAttach = ((uint)(7));
			w18.BottomAttach = ((uint)(8));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkbuttonDisplay = new global::Gtk.CheckButton ();
			this.checkbuttonDisplay.Sensitive = false;
			this.checkbuttonDisplay.CanFocus = true;
			this.checkbuttonDisplay.Name = "checkbuttonDisplay";
			this.checkbuttonDisplay.Label = global::Mono.Unix.Catalog.GetString ("Отображать в главной таблице");
			this.checkbuttonDisplay.DrawIndicator = true;
			this.checkbuttonDisplay.UseUnderline = true;
			this.vbox2.Add (this.checkbuttonDisplay);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.checkbuttonDisplay]));
			w19.Position = 0;
			w19.Expand = false;
			w19.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.checkbuttonSearch = new global::Gtk.CheckButton ();
			this.checkbuttonSearch.Sensitive = false;
			this.checkbuttonSearch.CanFocus = true;
			this.checkbuttonSearch.Name = "checkbuttonSearch";
			this.checkbuttonSearch.Label = global::Mono.Unix.Catalog.GetString ("Поиск по полю");
			this.checkbuttonSearch.DrawIndicator = true;
			this.checkbuttonSearch.UseUnderline = true;
			this.vbox2.Add (this.checkbuttonSearch);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.checkbuttonSearch]));
			w20.Position = 1;
			w20.Expand = false;
			w20.Fill = false;
			this.table1.Add (this.vbox2);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.table1 [this.vbox2]));
			w21.TopAttach = ((uint)(3));
			w21.BottomAttach = ((uint)(4));
			w21.LeftAttach = ((uint)(1));
			w21.RightAttach = ((uint)(2));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			w1.Add (this.table1);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(w1 [this.table1]));
			w22.Position = 0;
			// Internal child QSCustomFields.CFFieldPropertys.ActionArea
			global::Gtk.HButtonBox w23 = this.ActionArea;
			w23.Name = "dialog1_ActionArea";
			w23.Spacing = 10;
			w23.BorderWidth = ((uint)(5));
			w23.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w24 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w23 [this.buttonCancel]));
			w24.Expand = false;
			w24.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.Sensitive = false;
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w25 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w23 [this.buttonOk]));
			w25.Position = 1;
			w25.Expand = false;
			w25.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 344;
			this.DefaultHeight = 404;
			this.Show ();
			this.entryName.Changed += new global::System.EventHandler (this.OnEntryNameChanged);
			this.entryDBName.Changed += new global::System.EventHandler (this.OnEntryDBNameChanged);
			this.comboFieldType.Changed += new global::System.EventHandler (this.OnComboFieldTypeChanged);
			this.comboDataType.Changed += new global::System.EventHandler (this.OnComboDataTypeChanged);
			this.buttonOk.Clicked += new global::System.EventHandler (this.OnButtonOkClicked);
		}
	}
}

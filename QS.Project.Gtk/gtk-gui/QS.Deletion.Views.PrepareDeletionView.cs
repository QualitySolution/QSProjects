
// This file has been generated by the GUI designer. Do not modify.
namespace QS.Deletion.Views
{
	public partial class PrepareDeletionView
	{
		private global::Gtk.VBox vbox2;

		private global::Gamma.GtkWidgets.yLabel ylabelOperation;

		private global::Gamma.GtkWidgets.yLabel ylabel10;

		private global::Gtk.Table table1;

		private global::Gamma.GtkWidgets.yLabel ylabel2;

		private global::Gamma.GtkWidgets.yLabel ylabel3;

		private global::Gamma.GtkWidgets.yLabel ylabel6;

		private global::Gamma.GtkWidgets.yLabel ylabel7;

		private global::Gamma.GtkWidgets.yLabel ylabelLinks;

		private global::Gamma.GtkWidgets.yLabel ylabelToClean;

		private global::Gamma.GtkWidgets.yLabel ylabelToDelete;

		private global::Gamma.GtkWidgets.yLabel ylabelToRemoveFrom;

		private global::Gtk.Button buttonCancel;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget QS.Deletion.Views.PrepareDeletionView
			global::Stetic.BinContainer.Attach(this);
			this.Name = "QS.Deletion.Views.PrepareDeletionView";
			// Container child QS.Deletion.Views.PrepareDeletionView.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ylabelOperation = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelOperation.Name = "ylabelOperation";
			this.ylabelOperation.LabelProp = global::Mono.Unix.Catalog.GetString("ylabel1");
			this.vbox2.Add(this.ylabelOperation);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ylabelOperation]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.ylabel10 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel10.Name = "ylabel10";
			this.ylabel10.Xalign = 0F;
			this.ylabel10.LabelProp = global::Mono.Unix.Catalog.GetString("Найдено:");
			this.vbox2.Add(this.ylabel10);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.ylabel10]));
			w2.Position = 1;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(2)), ((uint)(4)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel2 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel2.Name = "ylabel2";
			this.ylabel2.Xalign = 1F;
			this.ylabel2.LabelProp = global::Mono.Unix.Catalog.GetString("Объектов для удаления:");
			this.table1.Add(this.ylabel2);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel2]));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel3 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel3.Name = "ylabel3";
			this.ylabel3.Xalign = 1F;
			this.ylabel3.LabelProp = global::Mono.Unix.Catalog.GetString("Ссылок для очистки:");
			this.table1.Add(this.ylabel3);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel3]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel6 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel6.Name = "ylabel6";
			this.ylabel6.Xalign = 1F;
			this.ylabel6.LabelProp = global::Mono.Unix.Catalog.GetString("Всего ссылок:");
			this.table1.Add(this.ylabel6);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel6]));
			w5.TopAttach = ((uint)(1));
			w5.BottomAttach = ((uint)(2));
			w5.LeftAttach = ((uint)(2));
			w5.RightAttach = ((uint)(3));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabel7 = new global::Gamma.GtkWidgets.yLabel();
			this.ylabel7.Name = "ylabel7";
			this.ylabel7.Xalign = 1F;
			this.ylabel7.LabelProp = global::Mono.Unix.Catalog.GetString("Объектов в коллекциях:");
			this.table1.Add(this.ylabel7);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabel7]));
			w6.LeftAttach = ((uint)(2));
			w6.RightAttach = ((uint)(3));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelLinks = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelLinks.Name = "ylabelLinks";
			this.ylabelLinks.LabelProp = global::Mono.Unix.Catalog.GetString("0");
			this.table1.Add(this.ylabelLinks);
			global::Gtk.Table.TableChild w7 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelLinks]));
			w7.TopAttach = ((uint)(1));
			w7.BottomAttach = ((uint)(2));
			w7.LeftAttach = ((uint)(3));
			w7.RightAttach = ((uint)(4));
			w7.XOptions = ((global::Gtk.AttachOptions)(4));
			w7.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelToClean = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelToClean.Name = "ylabelToClean";
			this.ylabelToClean.LabelProp = global::Mono.Unix.Catalog.GetString("0");
			this.table1.Add(this.ylabelToClean);
			global::Gtk.Table.TableChild w8 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelToClean]));
			w8.TopAttach = ((uint)(1));
			w8.BottomAttach = ((uint)(2));
			w8.LeftAttach = ((uint)(1));
			w8.RightAttach = ((uint)(2));
			w8.XOptions = ((global::Gtk.AttachOptions)(4));
			w8.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelToDelete = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelToDelete.Name = "ylabelToDelete";
			this.ylabelToDelete.LabelProp = global::Mono.Unix.Catalog.GetString("0");
			this.table1.Add(this.ylabelToDelete);
			global::Gtk.Table.TableChild w9 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelToDelete]));
			w9.LeftAttach = ((uint)(1));
			w9.RightAttach = ((uint)(2));
			w9.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.ylabelToRemoveFrom = new global::Gamma.GtkWidgets.yLabel();
			this.ylabelToRemoveFrom.Name = "ylabelToRemoveFrom";
			this.ylabelToRemoveFrom.LabelProp = global::Mono.Unix.Catalog.GetString("0");
			this.table1.Add(this.ylabelToRemoveFrom);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.ylabelToRemoveFrom]));
			w10.LeftAttach = ((uint)(3));
			w10.RightAttach = ((uint)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox2.Add(this.table1);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.table1]));
			w11.Position = 2;
			w11.Expand = false;
			w11.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.vbox2.Add(this.buttonCancel);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.buttonCancel]));
			w12.Position = 3;
			w12.Expand = false;
			w12.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
			this.buttonCancel.Clicked += new global::System.EventHandler(this.OnButtonCancelClicked);
		}
	}
}

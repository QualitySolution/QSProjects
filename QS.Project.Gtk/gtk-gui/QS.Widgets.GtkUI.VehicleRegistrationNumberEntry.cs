
// This file has been generated by the GUI designer. Do not modify.
namespace QS.Widgets.GtkUI
{
	public partial class VehicleRegistrationNumberEntry
	{
		private global::Gtk.HBox hbox1;

		private global::Gamma.Widgets.yEnumComboBox comboCountry;

		private global::Gtk.Entry entryNumber;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget QS.Widgets.GtkUI.VehicleRegistrationNumberEntry
			global::Stetic.BinContainer.Attach(this);
			this.Name = "QS.Widgets.GtkUI.VehicleRegistrationNumberEntry";
			// Container child QS.Widgets.GtkUI.VehicleRegistrationNumberEntry.Gtk.Container+ContainerChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.comboCountry = new global::Gamma.Widgets.yEnumComboBox();
			this.comboCountry.Name = "comboCountry";
			this.comboCountry.ShowSpecialStateAll = false;
			this.comboCountry.ShowSpecialStateNot = false;
			this.comboCountry.UseShortTitle = false;
			this.comboCountry.DefaultFirst = false;
			this.hbox1.Add(this.comboCountry);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.comboCountry]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.entryNumber = new global::Gtk.Entry();
			this.entryNumber.CanFocus = true;
			this.entryNumber.Name = "entryNumber";
			this.entryNumber.IsEditable = true;
			this.entryNumber.InvisibleChar = '•';
			this.hbox1.Add(this.entryNumber);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.entryNumber]));
			w2.Position = 1;
			this.Add(this.hbox1);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Hide();
		}
	}
}

using System;
namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class LegalNameAlternative : Gtk.Bin
	{
		public event EventHandler NameChanged;
		public event EventHandler OwnershipChanged;
		public event EventHandler FullNameChanged;

		public string Ownership {
			get {
				return String.IsNullOrWhiteSpace(comboOwnership.ActiveText) ? null : comboOwnership.ActiveText;
			}
			set {
				if(value == null) {
					comboOwnership.Active = 0;
					return;
				}
				int i = -1;
				foreach(object[] row in (comboOwnership.Model as Gtk.ListStore)) {
					i++;
					if(row[0] as string == value)
						comboOwnership.Active = i;
				}
			}
		}

		public string OwnName {
			get {
				return entryName.Text;
			}
			set {
				entryName.Text = value;
			}
		}

		public string FullName {
			get {
				return Ownership != null ? String.Format("{0} \"{1}\"", Ownership, OwnName) : OwnName;
			}
		}

		protected void OnNameChanged()
		{
			if(NameChanged != null)
				NameChanged(this, EventArgs.Empty);
		}

		protected void OnOwnershipChanged()
		{
			if(OwnershipChanged != null)
				OwnershipChanged(this, EventArgs.Empty);
		}

		protected void OnFullNameChanged()
		{
			if(FullNameChanged != null)
				FullNameChanged(this, EventArgs.Empty);
		}

		public LegalNameAlternative()
		{
			this.Build();

			comboOwnership.AppendText(String.Empty);
			foreach(var pair in CommonValues.Ownerships) {
				comboOwnership.AppendText(pair.Key);
			}
		}

		protected void OnComboOwnershipChanged(object sender, EventArgs e)
		{
			OnOwnershipChanged();
			OnFullNameChanged();
		}

		protected void OnEntryNameChanged(object sender, EventArgs e)
		{
			OnNameChanged();
			OnFullNameChanged();
		}
	}
}

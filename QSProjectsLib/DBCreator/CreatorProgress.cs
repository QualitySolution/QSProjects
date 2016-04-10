using System;

namespace QSProjectsLib
{
	public partial class CreatorProgress : Gtk.Window
	{
		public string OperationText{
			get{
				return progressCreation.Text;
			}
			set{
				progressCreation.Text = value;
				QSMain.WaitRedraw ();
			}
		}

		public double OperationPartCount{
			get{
				return progressCreation.Adjustment.Upper;
			}
			set{
				progressCreation.Adjustment.Upper = value;
			}
		}

		public double OperationCurPart{
			get{
				return progressCreation.Adjustment.Value;
			}
			set{
				progressCreation.Adjustment.Value = value;
				QSMain.WaitRedraw ();
			}
		}

		public CreatorProgress () :
		base (Gtk.WindowType.Popup)
		{
			this.Build ();
			SetPosition (Gtk.WindowPosition.CenterAlways);
		}
	}
}


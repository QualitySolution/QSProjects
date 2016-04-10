using System;
using Gtk;

namespace QSProjectsLib
{
	public partial class GetPassword : Gtk.Dialog
	{
		public string Login {
			get{
				return entryLogin.Text;
			}
		}
			
		public string Password{
			get{
				return entryPassword.Text;
			}
		}

		public GetPassword (string login)
		{
			this.Build ();
			entryLogin.Text = login;
		}

		protected void OnEntryPasswordActivated(object sender, EventArgs e)
		{
			buttonOk.Activate ();
		}

		protected void OnEntryLoginActivated(object sender, EventArgs e)
		{
			this.ChildFocus (DirectionType.TabForward);
		}
	}
}


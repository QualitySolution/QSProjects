using System;
using System.IO;
using Gtk;
using QSProjectsLib;

namespace QSBanks
{
	public partial class BanksUpdateWindow : Gtk.Window
	{
		BanksUpdater updater = new BanksUpdater(new QS.BaseParameters.ParametersService(QSMain.ConnectionDB));

		public BanksUpdateWindow() :
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
			this.WidthRequest = 400;
			this.HeightRequest = 300;
			vboxManualLoad.Visible = checkbutton.Active = false;
			var txtFilter = new FileFilter();
			txtFilter.AddPattern("*.zip");
			txtFilter.Name = "Zip архив (*.zip)";
			filechooser.AddFilter(txtFilter);
			filechooser.SelectMultiple = false;
			progressbar.Adjustment = new Adjustment(0, 0, 100, 1, 1, 1);
			updater.OnOutputMessage += Updater_OnOutputMessage;
			updater.OnProgress += Updater_OnProgress;;
		}

		protected void OnCheckbuttonToggled(object sender, EventArgs e)
		{
			vboxManualLoad.Visible = checkbutton.Active;
		}

		protected void OnButtonUpdateClicked(object sender, EventArgs e)
		{
			textviewOutput.Buffer.Clear();
			progressbar.Adjustment.Value = 0;
			if(checkbutton.Active && !string.IsNullOrWhiteSpace(filechooser.Filename) && File.Exists(filechooser.Filename)) {
				FileStream fs = new FileStream(filechooser.Filename, FileMode.Open);
				updater.UpdateBanks(fs);
			} else {
				updater.UpdateBanks();
			}
		}

		void Updater_OnOutputMessage(object sender, string e)
		{
			textviewOutput.Buffer.Text += e + "\n";
			QSMain.WaitRedraw();
		}

		void Updater_OnProgress(object sender, int e)
		{
			progressbar.Adjustment.Value = e;
			QSMain.WaitRedraw();
		}

	}
}

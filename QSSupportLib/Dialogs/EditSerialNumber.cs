using System;
using QSSupportLib.Serial;
using QSProjectsLib;

namespace QSSupportLib
{
	public partial class EditSerialNumber : Gtk.Dialog
	{
		private SNEncoder SerialNumberEncoder;

		public EditSerialNumber()
		{
			this.Build();

			SerialNumberEncoder = new SNEncoder();
			if (MainSupport.BaseParameters.SerialNumber != null)
				serialnumberentry1.Text = MainSupport.BaseParameters.SerialNumber;
		}

		protected void OnSerialnumberentry1Changed(object sender, EventArgs e)
		{
			SerialNumberEncoder.Number = serialnumberentry1.Text;

			//Сброс
			bool serialOk = false;
			labelResult.Text = String.Empty;

			if (SerialNumberEncoder.IsNotSupport)
				labelResult.Text = "Формат серийного номера не поддерживается этой верией программы.\n" +
				"Если вы уверены что серийный номер правильный, попробуйте обновить программу.";
			else if(SerialNumberEncoder.IsValid)
			{
				if (MainSupport.ProjectVerion.Product.ToLower() == SerialNumberEncoder.Product)
					serialOk = true;
				else
					labelResult.Text = "Серийный номер от другого продукта.";
			}

			buttonOk.Sensitive = serialOk;

			//Для возможности удалить из базы введенный серийный номер.
			if(String.IsNullOrWhiteSpace(serialnumberentry1.Text) && MainSupport.BaseParameters.SerialNumber != null)
			{
				buttonOk.Sensitive = true;
			}
		}

		public static void RunDialog()
		{
			var dlg = new EditSerialNumber();
			dlg.Show();
			dlg.Run();
			dlg.Destroy();
		}

		protected void OnButtonOkClicked(object sender, EventArgs e)
		{
			if(MainSupport.BaseParameters.SerialNumber != serialnumberentry1.Text)
			{
				if(String.IsNullOrWhiteSpace(serialnumberentry1.Text))
					MainSupport.BaseParameters.RemoveParameter(QSMain.ConnectionDB, "serial_number");
				else
					MainSupport.BaseParameters.UpdateParameter(QSMain.ConnectionDB, "serial_number", serialnumberentry1.Text);
			}
		}
	}
}


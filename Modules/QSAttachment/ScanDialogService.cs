using System;
using Gtk;
using QS.Attachments;

namespace QSAttachment
{
	public class ScanDialogService : IScanDialogService
	{
		public bool GetFileFromDialog(out string fileName, out byte[] file)
		{
			fileName = string.Empty;
			file = Array.Empty<byte>();

			var scanDialog = new GetFromScanner();

			scanDialog.Show();

			if(scanDialog.Run() == (int)ResponseType.Ok)
			{
				fileName = scanDialog.FileName;
				file = scanDialog.File;
			}

			scanDialog.Destroy();

			return !string.IsNullOrEmpty(fileName);
		}
	}
}
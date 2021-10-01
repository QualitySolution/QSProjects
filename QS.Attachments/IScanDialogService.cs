namespace QS.Attachments
{
	public interface IScanDialogService
	{
		bool GetFileFromDialog(out string fileName, out byte[] file);
	}
}
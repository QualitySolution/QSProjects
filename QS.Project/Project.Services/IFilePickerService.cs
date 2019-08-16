using QS.Tdi;

namespace QS.Project.Services
{
	public interface IFilePickerService
	{
		bool OpenSaveFilePicker(string fileName, out string filePath);
		bool OpenSaveFilePicker(string fileName, out string filePath, params string[] MIMEFilter);

		bool OpenSelectFilePicker(out string filePath);
		bool OpenSelectFilePicker(out string filePath, params string[] MIMEFilter);
	}
}

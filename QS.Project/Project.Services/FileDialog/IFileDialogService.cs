namespace QS.Project.Services.FileDialog
{
    public interface IFileDialogService
	{
		IDialogResult RunOpenFileDialog();
		IDialogResult RunOpenFileDialog(DialogSettings dialogSettings);
		IDialogResult RunSaveFileDialog();
		IDialogResult RunSaveFileDialog(DialogSettings dialogSettings);
		IDialogResult RunOpenDirectoryDialog();
		IDialogResult RunOpenDirectoryDialog(DialogSettings dialogSettings);
	}
}

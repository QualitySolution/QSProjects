using System;
using System.Data.Bindings.Collections;
using System.Diagnostics;
using System.IO;
using NLog;
using QS.Commands;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.ViewModels;

namespace QS.Attachments.ViewModels.Widgets
{
	public class AttachmentsViewModel : WidgetViewModelBase
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private const int _maxFileNameLength = 45;
		private readonly IAttachmentFactory _attachmentFactory;
		private readonly IFilePickerService _filePickerService;
		private readonly IScanDialogService _scanDialogService;
		private readonly int _currentUserId;
		private IAttachment _selectedAttachment;

		private DelegateCommand _addCommand;
		private DelegateCommand _openCommand;
		private DelegateCommand _saveCommand;
		private DelegateCommand _deleteCommand;
		private DelegateCommand _scanCommand;
		
		public AttachmentsViewModel(
			IAttachmentFactory attachmentFactory,
			IFilePickerService filePickerService,
			IScanDialogService scanDialogService,
			int currentUserId,
			ICovarianceList<IAttachment> attachments)
		{
			_attachmentFactory = attachmentFactory ?? throw new ArgumentNullException(nameof(attachmentFactory));
			_filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
			_scanDialogService = scanDialogService ?? throw new ArgumentNullException(nameof(scanDialogService));
			_currentUserId = currentUserId;
			Attachments = attachments;
		}

		public ICovarianceList<IAttachment> Attachments { get; }

		public IAttachment SelectedAttachment
		{
			get => _selectedAttachment;
			set
			{
				if(SetField(ref _selectedAttachment, value))
				{
					OnPropertyChanged(nameof(CanSave));
					OnPropertyChanged(nameof(CanDelete));
					OnPropertyChanged(nameof(CanOpen));
				}
			}
		}

		public bool CanSave => SelectedAttachment != null;
		public bool CanDelete => SelectedAttachment != null;
		public bool CanOpen => SelectedAttachment != null;

		public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(
				() =>
				{
					if(_filePickerService.OpenAttachFilePicker(out string filePath))
					{
						_logger.Info("Чтение файла...");
						byte[] file = File.ReadAllBytes(filePath);
						var attachment = _attachmentFactory.CreateNewAttachment(GetValidFileName(filePath), file);
						Attachments.Add(attachment);
						_logger.Info("Ok");
					}
				}
			)
		);

		public DelegateCommand OpenCommand => _openCommand ?? (_openCommand = new DelegateCommand(
				() =>
				{
					var vodUserTempDir = UserRepository.GetTempDirForCurrentUser(_currentUserId, SelectedAttachment.PartOfPath);

					if(string.IsNullOrWhiteSpace(vodUserTempDir))
					{
						return;
					}

					var tempFilePath = Path.Combine(Path.GetTempPath(), vodUserTempDir, SelectedAttachment.FileName);

					if(!File.Exists(tempFilePath))
					{
						File.WriteAllBytes(tempFilePath, SelectedAttachment.ByteFile);
					}

					var process = new Process();
					process.StartInfo.FileName = Path.Combine(vodUserTempDir, SelectedAttachment.FileName);
					process.Start();
				}
			)
		);

		public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(
				() =>
				{
					if(_filePickerService.OpenSaveFilePicker(SelectedAttachment.FileName, out string filePath))
					{
						_logger.Info("Сохраняем файл на диск...");
						File.WriteAllBytes(filePath, SelectedAttachment.ByteFile);
						_logger.Info("Ок");
					}
				}
			)
		);

		public DelegateCommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new DelegateCommand(
				() =>
				{
					if(Attachments.Contains(SelectedAttachment))
					{
						Attachments.Remove(SelectedAttachment);
					}
				}
			)
		);

		public DelegateCommand ScanCommand => _scanCommand ?? (_scanCommand = new DelegateCommand(
				() =>
				{
					if(_scanDialogService.GetFileFromDialog(out string fileName, out byte[] file))
					{
						var attachment = _attachmentFactory.CreateNewAttachment(GetValidFileName(fileName), file);
						Attachments.Add(attachment);
					}
				}
			)
		);

		private string GetValidFileName(string fileName)
		{
			var attachedFileName = Path.GetFileName(fileName);

			//При необходимости обрезаем имя файла.
			if(attachedFileName.Length > _maxFileNameLength)
			{
				var ext = Path.GetExtension(attachedFileName);
				var name = Path.GetFileNameWithoutExtension(attachedFileName);
				attachedFileName = $"{name.Remove(_maxFileNameLength - ext.Length)}{ext}";
			}

			return attachedFileName;
		}
	}
}

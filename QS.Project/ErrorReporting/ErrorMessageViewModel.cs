using System;
using QS.Commands;
using QS.Dialog;
using QS.ViewModels;

namespace QS.ErrorReporting
{
	public class ErrorMessageViewModel : ViewModelBase
	{
		public ErrorMessageViewModel(ErrorMessageModelBase errorMessageModel, IInteractiveMessage interactiveMessage)
		{
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.errorMessageModel = errorMessageModel ?? throw new ArgumentNullException(nameof(errorMessageModel));
			errorMessageModel.PropertyChanged += (sender, e) => {
				OnPropertyChanged(nameof(CanSendErrorReport));
				OnPropertyChanged(nameof(IsEmailValid));
			};
			CreateSendReportCommand();
		}

		protected ErrorMessageModelBase errorMessageModel;
		protected IInteractiveMessage interactiveMessage;

		public string ErrorData => errorMessageModel.ErrorData;
		public string ExceptionText => errorMessageModel.ExceptionText;
		public bool CanSendErrorReport => errorMessageModel.CanSendErrorReportManually;
		public bool IsEmailValid => errorMessageModel.IsEmailValid;
		public bool ReportSent { get; protected set; }

		private string desciption;
		public string Description {
			get => desciption;
			set { 
				if(SetField(ref desciption, value, () => Description))
					errorMessageModel.Description = desciption;
			 }
		}

		private string email;
		public string Email {
			get => email;
			set {
				if(SetField(ref email, value, () => Email))
					errorMessageModel.Email = email;
			}
		}

		public void AddException(Exception exception)
		{
			errorMessageModel.Exceptions.Add(exception);
		}

		public void SendReportAutomatically()
		{
			if(errorMessageModel.CanSendErrorReportAutomatically)
				SendReportCommand.Execute(ErrorReportType.Automatic);
		}

		public DelegateCommand<ErrorReportType> SendReportCommand;
		private void CreateSendReportCommand()
		{
			SendReportCommand = new DelegateCommand<ErrorReportType>(
				(errorReportType) => {
					if(ReportSent)
						return;
					errorMessageModel.ErrorReportType = errorReportType;

					var result = errorMessageModel.SendErrorReport();
					if(result)
						ReportSent = true;
					else
						interactiveMessage.ShowMessage(ImportanceLevel.Warning, "Отправка сообщения не удалась.\n" +
							"Проверьте ваше интернет соединение и повторите попытку. " +
							"Если отправка неудастся возможно имеются проблемы на стороне сервера."
						);
				},
				(errorReportType) => CanSendErrorReport
			);
		}
	}
}

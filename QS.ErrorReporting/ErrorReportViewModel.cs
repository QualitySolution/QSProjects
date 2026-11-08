using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using QS.Project.DB;
using QS.Project.Versioning;

namespace QS.ErrorReporting {
	/// <summary>Состояние и формирование отчёта; не зависит от интерфейса и навигации.</summary>
	public class ErrorReportViewModel : INotifyPropertyChanged {
		private static readonly Regex emailPattern = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");
		private readonly IApplicationInfo application;
		private readonly IDataBaseInfo database;
		private readonly IUserInfo user;
		private readonly IErrorReportingSettings settings;
		private readonly ILogService log;
		private readonly Func<SubmitErrorRequest, bool> send;
		private readonly List<Exception> exceptions = new List<Exception>();
		private string email;
		private string description = String.Empty;
		private bool reportSent;

		public ErrorReportViewModel(Exception exception, IApplicationInfo application, IUserInfo user,
			IErrorReportingSettings settings, ILogService log, IDataBaseInfo database = null,
			Func<SubmitErrorRequest, bool> send = null) {
			this.application = application ?? throw new ArgumentNullException(nameof(application));
			this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
			this.log = log ?? throw new ArgumentNullException(nameof(log));
			this.user = user;
			this.database = database;
			this.send = send ?? SendReport;
			email = user?.Email ?? String.Empty;
			AddException(exception);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public string Email { get => email; set { email = value ?? String.Empty; Changed(nameof(Email)); Changed(nameof(IsEmailValid)); Changed(nameof(CanSend)); Changed(nameof(ValidationMessage)); } }
		public string Description { get => description; set { description = value ?? String.Empty; Changed(nameof(Description)); Changed(nameof(CanSend)); Changed(nameof(ValidationMessage)); } }
		public bool IsEmailValid => emailPattern.IsMatch(Email);
		public bool CanSend => (!settings.RequestDescription || !String.IsNullOrWhiteSpace(Description)) && (!settings.RequestEmail || IsEmailValid);
		public string ValidationMessage {
			get {
				bool missingDescription = settings.RequestDescription && String.IsNullOrWhiteSpace(Description);
				bool invalidEmail = settings.RequestEmail && !IsEmailValid;
				if(missingDescription && invalidEmail) return "Заполните описание и укажите корректный email.";
				if(missingDescription) return "Заполните описание ошибки.";
				if(invalidEmail) return "Укажите корректный email для связи.";
				return String.Empty;
			}
		}
		public string ExceptionText => String.Join("\n Следующее исключение:\n", exceptions.Select(x => x.ToString()));
		public string CopyText => $"Продукт: {application.ProductName}\nВерсия: {application.Version}\nРедакция: {application.Modification}\nОшибка: {ExceptionText}";
		public bool ReportSent => reportSent;
		public bool SendAutomatically => settings.SendAutomatically;

		public void AddException(Exception exception) {
			exceptions.Add(exception ?? throw new ArgumentNullException(nameof(exception)));
			Changed(nameof(ExceptionText));
			Changed(nameof(CopyText));
		}

		public bool Submit(ReportType type) {
			var request = new SubmitErrorRequest {
				App = new AppInfo { ProductCode = application.ProductCode, Modification = application.Modification ?? String.Empty, Version = application.Version?.ToString() ?? String.Empty },
				Db = new DatabaseInfo { Name = database?.Name ?? String.Empty },
				User = new UserInfo { Email = Email, Name = user?.Name ?? String.Empty },
				Report = new ErrorInfo { StackTrace = ExceptionText, UserDescription = Description, Log = log.GetLog(type == ReportType.Automatic ? settings.LogRowCount : null) ?? String.Empty },
				ReportType = type
			};
			if(!send(request)) return false;
			reportSent = true;
			return true;
		}

		private static bool SendReport(SubmitErrorRequest request) {
			using(var service = new ErrorReportingService()) return service.SubmitErrorReport(request);
		}
		private void Changed(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}

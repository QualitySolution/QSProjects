using System;
using NLog;
using QS.Services;
using QS.Utilities.Text;
using QS.ViewModels;

namespace QS.Dialog.ViewModels {
	public class StatusBarViewModel : ViewModelBase {
		private readonly ITrackerActionInvoker invoker;
		private readonly IUserService userService;

		public StatusBarViewModel(ITrackerActionInvoker invoker, IUserService userService) {
			this.invoker = invoker;
			this.userService = userService;
			RegisterTargetForNlog();
		}

		/// <summary>
		/// Регистрируем правила Nlog для строки состояния
		/// </summary>
		public void RegisterTargetForNlog ()
		{
			NLog.Config.LoggingConfiguration config = NLog.LogManager.Configuration;
			if (config.FindTargetByName ("status") != null)
				return;
			NLog.Targets.MethodCallTarget targetLog = new NLog.Targets.MethodCallTarget (
				"status", 
				(info, objects) => SetStatusText(info.Message)
			);
			config.AddTarget ("status", targetLog);
			NLog.Config.LoggingRule rule = new NLog.Config.LoggingRule ("*", targetLog);
			rule.EnableLoggingForLevel (LogLevel.Info);
			config.LoggingRules.Add (rule);

			LogManager.Configuration = config;
		}
		
		private string statusText;
		public virtual string StatusText {
			get => statusText;
			protected set => SetField (ref statusText, value);
		}
		
		public string UserName => userService.GetCurrentUser()?.Name;

		private void SetStatusText (string text) {
			invoker.Invoke(() => StatusText = text.EllipsizeMiddle(160));
		}
	}
}

using System;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Services;
using QS.Navigation;
using QS.Navigation.TabNavigation.TdiAdapter;
using QS.Navigation.TabNavigation;

namespace QS.ViewModels
{
	public abstract class ViewModelBase : PropertyChangedBase
	{
		private readonly IInteractiveService interactiveService;

		public INavigationManager NavigationManager { get; internal set; }
		//public ITdiCompatibilityNavigation TdiCompatibilityNavigation { get; internal set; }

		protected ViewModelBase(IInteractiveService interactiveService)
		{
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		protected virtual void ShowInfoMessage(string message, string title = null)
		{
			interactiveService.InteractiveMessage.ShowMessage(ImportanceLevel.Info, message, title);
		}

		protected virtual void ShowWarningMessage(string message, string title = null)
		{
			interactiveService.InteractiveMessage.ShowMessage(ImportanceLevel.Warning, message, title);
		}

		protected virtual void ShowErrorMessage(string message, string title = null)
		{
			interactiveService.InteractiveMessage.ShowMessage(ImportanceLevel.Error, message, title);
		}

		protected virtual bool AskQuestion(string question, string title = null)
		{
			return interactiveService.InteractiveQuestion.Question(question, title);
		}

	}
}

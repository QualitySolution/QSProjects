using System;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
namespace QS.Navigation.TabNavigation
{
	public abstract class TabPageViewModelBase : ViewModelBase
	{
		private readonly INavigationManager navigationManager;

		public IPage Page { get; }

		public TabPageViewModelBase(IPage page, INavigationManager navigationManager, IInteractiveService interactiveService) : base(interactiveService)
		{
			Page = page ?? throw new ArgumentNullException(nameof(page));
			this.navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			if(Page.ViewModel is TabViewModelBase tabViewModel) {
				tabViewModel.TabNameChanged += (sender, e) => Title = tabViewModel.TabName;
				Title = tabViewModel.TabName;
			}
		}

		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value, () => Title);
		}

		private DelegateCommand closeCommand;
		public DelegateCommand CloseCommand {
			get {
				if(closeCommand == null) {
					closeCommand = new DelegateCommand(
						() => navigationManager.AskClosePage(Page),
						//Определить ниже условия для контроля блокировки кнопки закрытия вкладки
						() => true
					);
				}
				return closeCommand;
			}
		}
	}
}

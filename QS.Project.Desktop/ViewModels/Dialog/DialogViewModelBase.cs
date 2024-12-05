using QS.Navigation;

namespace QS.ViewModels.Dialog
{
	/// <summary>
	/// Базовый класс для всех ViewModel представляющих собой диалоги. Это либо вкладки, либо окна в интерфейсе. 
	/// Которые пользователь открывает для совершения каких-то отдельных действий.
	/// Внутри кода, такие диалоги открываются через INavigationManager
	/// </summary>
	public abstract class DialogViewModelBase : ViewModelBase
	{
		public INavigationManager NavigationManager { get; set; }

		protected DialogViewModelBase(INavigationManager navigation)
		{
			//FIXME Когда выпилим ViewModel с TDI, добавить проверку на null;
			this.NavigationManager = navigation;
		}

		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		public virtual void Close(bool askClose, CloseSource source)
		{
			var page = NavigationManager?.FindPage(this);
			if(page != null) {
				if(askClose)
					NavigationManager.AskClosePage(page, source);
				else
					NavigationManager.ForceClosePage(page, source);
			}
		}
	}
}

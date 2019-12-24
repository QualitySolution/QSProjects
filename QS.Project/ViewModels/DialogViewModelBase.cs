using System;
using QS.Navigation;
using QS.Services;

namespace QS.ViewModels
{
	/// <summary>
	/// Базовый класс для всех ViewModel представляющих из себя диалоги. Это либо вкладки, либо окна в интерфейсе. 
	/// Которые пользователь откывает для совершения каких-то отдельных действий.
	/// Внутри кода, такие диалоги открываются через INavigationManager
	/// </summary>
	public class DialogViewModelBase : ViewModelBase
	{
		//FIXME Когда выпилим ViewModel с TDI, добавить заполение свойства через конструктор;
		public INavigationManager NavigationManager { get; set; }

		public DialogViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
		}

		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		public virtual void Close(bool askClose)
		{
			var page = NavigationManager?.FindPage(this);
			if(page != null) {
				if(askClose)
					NavigationManager.AskClosePage(page);
				else
					NavigationManager.ForceClosePage(page);
			}
		}
	}
}

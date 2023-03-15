using System;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.Navigation;
using QS.ViewModels.Dialog;

namespace QS.Testing.Testing.Navigation {
	
	/// <summary>
	/// Пока не полная реализация прощенного менеджера навигации предназначенного для использования в тестах.
	/// Будет наполнять по необходимости.
	/// </summary>
	public class NavigationManagerForTests : NavigationManagerBase, INavigationManager {
		private AutofacViewModelsFactoryForTests viewModelsFactory;
		
		public NavigationManagerForTests(ILifetimeScope scope, IInteractiveMessage interactive, IPageHashGenerator hashGenerator = null) : base(interactive, hashGenerator) {
			viewModelsFactory = new AutofacViewModelsFactoryForTests(scope); //Здесь пока не усложняю с зависимостями.
		}

		public IPage<TViewModel> FindPage<TViewModel>() where TViewModel : DialogViewModelBase
			=> pages.OfType<IPage<TViewModel>>().FirstOrDefault();

		public IPage CurrentPage => TopLevelPages.LastOrDefault(); //Пока не реализовано переключение. Это самый простой вариант.

		public override void SwitchOn(IPage page) {
			throw new NotImplementedException();
		}

		public bool AskClosePage(IPage page, CloseSource source = CloseSource.External) {
			ClosePage(page, source);
			return true;
		}

		public void ForceClosePage(IPage page, CloseSource source = CloseSource.External) {
			ClosePage(page, source);
		}

		protected override IViewModelsPageFactory GetPageFactory<TViewModel>() {
			return viewModelsFactory;
		}

		protected override void OpenSlavePage(IPage masterPage, IPage page) {
			pages.Add(page);
		}

		protected override void OpenPage(IPage masterPage, IPage page) {
			pages.Add(page);
		}
	}
}

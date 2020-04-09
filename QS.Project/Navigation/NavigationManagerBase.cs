using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.ErrorReporting;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Navigation
{
	public abstract class NavigationManagerBase
	{
		protected readonly IPageHashGenerator hashGenerator;
		protected readonly IInteractiveMessage interactiveMessage;

		protected NavigationManagerBase(IInteractiveMessage interactive, IPageHashGenerator hashGenerator = null)
		{
			this.hashGenerator = hashGenerator;
			this.interactiveMessage = interactive ?? throw new ArgumentNullException(nameof(interactive));
		}

		#region Перебор страниц
		protected readonly List<IPage> pages = new List<IPage>();

		public IEnumerable<IPage> AllPages {
			get {
				foreach(var page in pages) {
					yield return page;
					foreach(var child in page.ChildPagesAll)
						yield return child.ChildPage;
				}
			}
		}

		public IEnumerable<IPage> TopLevelPages => pages;

		public IEnumerable<IPage> IndependentPages {
			get {
				foreach(var page in pages) {
					if(SlavePages.All(x => x.SlavePage != page))
						yield return page;
					foreach(var child in page.ChildPagesAll)
						if(SlavePages.All(x => x.SlavePage != child))
							yield return child.ChildPage;
				}
			}
		}

		public IEnumerable<MasterToSlavePair> SlavePages {
			get {
				foreach(var page in pages) {
					foreach(var slavePair in page.SlavePagesAll)
						yield return slavePair;
				}
			}
		}

		public IEnumerable<ParentToChildPair> ChildPages {
			get {
				foreach(var page in pages) {
					foreach(var childPair in page.ChildPagesAll)
						yield return childPair;
				}
			}
		}

		#endregion

		#region Поиск

		public IPage FindPage(DialogViewModelBase viewModel)
		{
			return AllPages.FirstOrDefault(x => x.ViewModel == viewModel);
		}

		//Внимание здесь специально параметр viewModel не является типом приведения результата TViewModel, так как если 
		//бы viewModel была типом TViewModel, то в вызове можно было бы не указывать Generic тип, а это бы приводило к 
		//попытке каста в IPage<DialogViewModelBase> при передаче не типизированного VM.
		public IPage<TViewModel> FindPage<TViewModel>(DialogViewModelBase viewModel) where TViewModel : DialogViewModelBase
		{
			return (IPage<TViewModel>)FindPage(viewModel);
		}

		#endregion

		#region Открытие

		public IPage<TViewModel> OpenViewModel<TViewModel>(DialogViewModelBase master, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var types = new Type[] { };
			var values = new object[] { };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options, addingRegistrations);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(DialogViewModelBase master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1) };
			var values = new object[] { arg1 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options, addingRegistrations);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(DialogViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2) };
			var values = new object[] { arg1, arg2 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options, addingRegistrations);
		}

		public IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(DialogViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, TCtorArg3 arg3, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			var types = new Type[] { typeof(TCtorArg1), typeof(TCtorArg2), typeof(TCtorArg3) };
			var values = new object[] { arg1, arg2, arg3 };
			return OpenViewModelTypedArgs<TViewModel>(master, types, values, options, addingRegistrations);
		}

		public IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			return (IPage<TViewModel>)OpenViewModelInternal(
				FindPage(master), options,
				() => hashGenerator?.GetHash<TViewModel>(master, ctorTypes, ctorValues),
				(hash) => GetPageFactory<TViewModel>().CreateViewModelTypedArgs<TViewModel>(master, ctorTypes, ctorValues, hash, addingRegistrations)
			);
		}

		public IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase
		{
			return (IPage<TViewModel>)OpenViewModelInternal(
				FindPage(master), options,
				() => hashGenerator?.GetHashNamedArgs<TViewModel>(master, ctorArgs),
				(hash) => GetPageFactory<TViewModel>().CreateViewModelNamedArgs<TViewModel>(master, ctorArgs, hash, addingRegistrations)
			);
		}

		#region Внутренне

		protected IPage OpenViewModelInternal(IPage masterPage, OpenPageOptions options, Func<string> makeHash, Func<string, IPage> makePage)
		{
			string hash = null;
			if(!options.HasFlag(OpenPageOptions.IgnoreHash))
				hash = makeHash();

			IPage openPage = null;

			if(options.HasFlag(OpenPageOptions.AsSlave)) {
				if(masterPage == null)
					throw new InvalidOperationException($"Отсутствует главная страница для добавляемой подчиненой страницы.");

				if(hash != null)
					openPage = masterPage.SlavePagesAll.Select(x => x.SlavePage).FirstOrDefault(x => x.PageHash == hash);
				if(openPage != null)
					SwitchOn(openPage);
				else {
					openPage = MakePageAndCatchAborting(makePage, hash);
					if (openPage == null)
						return null;
					(masterPage as IPageInternal).AddSlavePage(openPage);
					OpenSlavePage(masterPage, openPage);
				}
			} else {
				if(hash != null)
					openPage = IndependentPages.FirstOrDefault(x => x.PageHash == hash);
				if(openPage != null)
					SwitchOn(openPage);
				else {
					openPage = MakePageAndCatchAborting(makePage, hash);
					if (openPage == null)
						return null;
					OpenPage(masterPage, openPage);
				}
			}
			return openPage;
		}

		IPage MakePageAndCatchAborting(Func<string, IPage> makePage, string hash)
		{
			try {
				return makePage(hash);
			}
			catch (Exception ex) when (ex.FineExceptionTypeInInner<AbortCreatingPageException>() != null) {
				var abortEx = ex.FineExceptionTypeInInner<AbortCreatingPageException>();
				interactiveMessage.ShowMessage(ImportanceLevel.Error, abortEx.Message, abortEx.Title);
				return null;
			}
		}

		protected virtual void ClosePage(IPage page, CloseSource source)
		{
			if (page.ViewModel is IOnCloseActionViewModel onClose)
				onClose.OnClose(source);

			var closedPagePair = SlavePages.FirstOrDefault(x => x.SlavePage == page);
			if (closedPagePair != null)
				(closedPagePair.MasterPage as IPageInternal).RemoveSlavePage(closedPagePair.SlavePage);
			var pageToRemove = pages.FirstOrDefault(x => x == page);
			if (pageToRemove != null) {
				pages.Remove(pageToRemove);
				(pageToRemove as IPageInternal).OnClosed();
			}
			else {
				var childPair = ChildPages.FirstOrDefault(x => x.ChildPage == page);
				if (childPair != null) {
					(childPair.ParentPage as IPageInternal).RemoveChildPage(childPair.ChildPage);
					(childPair.ChildPage as IPageInternal).OnClosed();
				}
			}

			if (page.ViewModel is IDisposable pd)
				pd.Dispose();
		}
		#endregion

		#endregion

		#region Абстрактные методы
		public abstract void SwitchOn(IPage page);

		protected abstract IViewModelsPageFactory GetPageFactory<TViewModel>();
		protected abstract void OpenSlavePage(IPage masterPage, IPage page);
		protected abstract void OpenPage(IPage masterPage, IPage page);
		#endregion
	}
}

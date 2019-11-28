using System;
using System.Collections.Generic;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Navigation
{
	public interface INavigationManager
	{
		/// <summary>
		/// Открывает TViewModel в интерфейсе пользователя, если ViewModel с таким же hash уже открыта, то просто 
		/// пеключает интерфейс на ее, иначе создает. Метод возвращает класс страницы, для того чтобы внешний код
		/// мог передать в созданную ViewModel дополнительные параметры, подписаться на какие либо события или передать
		/// кому-то ссылку на открытую страницу.
		/// </summary>
		/// <param name="master">Для подчиненых страниц, тут передается главная страница. Для обычного открытия может быть 
		/// передана страница после которой следует разместить созданную или null для размещения в конце списка.</param>
		/// <param name="options">Параметры режима открытия.</param>
		/// <typeparam name="TViewModel">Тип ViewModel которую необходимо создать.</typeparam>
		IPage<TViewModel> OpenViewModel<TViewModel>(ViewModelBase master, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(ViewModelBase master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(ViewModelBase master, TCtorArg1 arg1, TCtorArg1 arg2, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(ViewModelBase master, TCtorArg1 arg1, TCtorArg1 arg2, TCtorArg1 arg3, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase;

		IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase;
		IPage OpenViewModelTypedArgs(Type viewModelType, ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None);
		IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase;
		IPage OpenViewModelNamedArgs(Type viewModelType, ViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None);

		IPage<TViewModel> FindPage<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;

		void SwitchOn(IPage page);

		bool AskClosePage(IPage page);
		void ForceClosePage(IPage page);
	}

	/// <summary>
	/// Интерфейс специально созданный для переходного периода, пока есть микс диалогов базирующихся чисто на Tdi и ViewModels.
	/// Его не нужно реализовывать в не Tdi менеджерах. И нужно будет удалить когда чисто TDI диалогов не останется.
	/// </summary>
	public interface ITdiCompatibilityNavigation : INavigationManager
	{
		IPage<TViewModel> OpenViewModelOnTdi<TViewModel>(Tdi.ITdiTab master, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase;
		IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(Tdi.ITdiTab master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TViewModel : ViewModelBase;

		ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, OpenPageOptions options = OpenPageOptions.None) where TTab : ITdiTab;
		ITdiPage OpenTdiTab<TTab, TCtorArg1>(ITdiTab masterTab, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None) where TTab : ITdiTab;
		ITdiPage OpenTdiTab<TTab>(ITdiTab masterTab, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None) where TTab : ITdiTab;
	}

	[Flags]
	public enum OpenPageOptions
	{
		None = 0,
		AsSlave = 1,
		IgnoreHash = 2
	}
}

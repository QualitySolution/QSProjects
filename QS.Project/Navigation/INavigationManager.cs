using System;
using System.Collections.Generic;
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
		/// <param name="master">Для подчиненых страниц, тут передается главная страница. ДЛя обычного открытия может быть 
		/// передана страница после которой следует разместить созданную или null для размещения в конце списка.</param>
		/// <param name="options">Параметры режима открытия.</param>
		/// <typeparam name="TViewModel">Тип ViewModel которую необходимо создать.</typeparam>
		IPage<TViewModel> OpenViewModel<TViewModel>(ViewModelBase master, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(ViewModelBase master, TCtorArg1 arg1, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(ViewModelBase master, TCtorArg1 arg1, TCtorArg1 arg2, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(ViewModelBase master, TCtorArg1 arg1, TCtorArg1 arg2, TCtorArg1 arg3, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase;

		IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase;
		IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase;

		IPage<TViewModel> FindPage<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;

		void SwitchOn(IPage page);

		bool AskClosePage(IPage page);
		void ForceClosePage(IPage page);
	}

	/// <summary>
	/// Интерфейс специально созданный для переходного периода, пока есть микс диалогов базирующихся чисто на Tdi и ViewModels.
	/// Его не нужно реализовывать в не Tdi менеджерах. И нужно будет удалить когда чисто TDI диалогов не останется.
	/// </summary>
	public interface ITdiCompatibilityNavigation
	{
		IPage<TViewModel> OpenViewModelOnTdi<TViewModel>(Tdi.ITdiTab master, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase;
		IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(Tdi.ITdiTab master, Type[] ctorTypes, object[] ctorValues, OpenViewModelOptions options = OpenViewModelOptions.None) where TViewModel : ViewModelBase;
	}

	[Flags]
	public enum OpenViewModelOptions
	{
		None = 0,
		AsSlave = 1,
		IgnoreHash = 2
	}
}

using System;
using System.Collections.Generic;
using Autofac;
using QS.Tdi;
using QS.ViewModels.Dialog;

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
		IPage<TViewModel> OpenViewModel<TViewModel>(DialogViewModelBase master, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(DialogViewModelBase master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(DialogViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(DialogViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, TCtorArg3 arg3, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;

		IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;

		IPage FindPage(DialogViewModelBase viewModel);
		IPage<TViewModel> FindPage<TViewModel>(DialogViewModelBase viewModel) where TViewModel : DialogViewModelBase;

		IPage CurrentPage { get; }

		void SwitchOn(IPage page);

		bool AskClosePage(IPage page, CloseSource source = CloseSource.External);
		void ForceClosePage(IPage page, CloseSource source = CloseSource.External);
	}

	/// <summary>
	/// Интерфейс специально созданный для переходного периода, пока есть микс диалогов базирующихся чисто на Tdi и ViewModels.
	/// Его не нужно реализовывать в не Tdi менеджерах. И нужно будет удалить когда чисто TDI диалогов не останется.
	/// </summary>
	public interface ITdiCompatibilityNavigation : INavigationManager
	{
		IPage<TViewModel> OpenViewModelOnTdi<TViewModel>(Tdi.ITdiTab master, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModelOnTdi<TViewModel, TCtorArg1>(Tdi.ITdiTab master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModelOnTdi<TViewModel, TCtorArg1, TCtorArg2>(Tdi.ITdiTab master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(Tdi.ITdiTab master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;

		ITdiPage OpenTdiTabOnTdi<TTab>(ITdiTab masterTab, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTabOnTdi<TTab, TCtorArg1>(ITdiTab masterTab, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTabOnTdi<TTab, TCtorArg1, TCtorArg2>(ITdiTab masterTab, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTabOnTdi<TTab>(ITdiTab masterTab, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;

		ITdiPage OpenTdiTab<TTab>(DialogViewModelBase master, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTab<TTab, TCtorArg1>(DialogViewModelBase master, TCtorArg1 arg1, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTab<TTab, TCtorArg1, TCtorArg2>(DialogViewModelBase master, TCtorArg1 arg1, TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTab<TTab>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, OpenPageOptions options = OpenPageOptions.None, Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
	}

	[Flags]
	public enum OpenPageOptions
	{
		None = 0,
		AsSlave = 1,
		IgnoreHash = 2
	}
}

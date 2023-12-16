using System;
using System.Collections.Generic;
using Autofac;
using QS.ViewModels.Dialog;

namespace QS.Navigation
{
	public interface INavigationManager
	{
		/// <summary>
		/// Открывает TViewModel в интерфейсе пользователя, если ViewModel с таким же hash уже открыта, то просто 
		/// переключает интерфейс на ее, иначе создает. Метод возвращает класс страницы, для того чтобы внешний код
		/// мог передать в созданную ViewModel дополнительные параметры, подписаться на какие либо события или передать
		/// кому-то ссылку на открытую страницу.
		/// </summary>
		/// <param name="master">Для подчиненных страниц, тут передается главная страница. ДЛя обычного открытия может быть 
		/// передана страница после которой следует разместить созданную или null для размещения в конце списка.</param>
		/// <param name="options">Параметры режима открытия.</param>
		/// <typeparam name="TViewModel">Тип ViewModel которую необходимо создать.</typeparam>
		IPage<TViewModel> OpenViewModel<TViewModel>(
			DialogViewModelBase master,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1>(
			DialogViewModelBase master,
			TCtorArg1 arg1,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2>(
			DialogViewModelBase master,
			TCtorArg1 arg1,
			TCtorArg2 arg2,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModel<TViewModel, TCtorArg1, TCtorArg2, TCtorArg3>(
			DialogViewModelBase master,
			TCtorArg1 arg1,
			TCtorArg2 arg2,
			TCtorArg3 arg3,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;

		IPage<TViewModel> OpenViewModelTypedArgs<TViewModel>(
			DialogViewModelBase master,
			Type[] ctorTypes,
			object[] ctorValues,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModelNamedArgs<TViewModel>(
			DialogViewModelBase master,
			IDictionary<string, object> ctorArgs,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;

		IPage FindPage(DialogViewModelBase viewModel);
		IPage<TViewModel> FindPage<TViewModel>(DialogViewModelBase viewModel) where TViewModel : DialogViewModelBase;

		IPage CurrentPage { get; }

		void SwitchOn(IPage page);

		bool AskClosePage(IPage page, CloseSource source = CloseSource.External);
		void ForceClosePage(IPage page, CloseSource source = CloseSource.External);
	}

	[Flags]
	public enum OpenPageOptions
	{
		None = 0,
		AsSlave = 1,
		IgnoreHash = 2,
		AsSlaveIgnoreHash = 3
	}
}

using System;
using System.Collections.Generic;
using Autofac;
using QS.Tdi;
using QS.ViewModels.Dialog;

namespace QS.Navigation
{
	/// <summary>
	/// Интерфейс специально созданный для переходного периода, пока есть микс диалогов базирующихся чисто на Tdi и ViewModels.
	/// Его не нужно реализовывать в не Tdi менеджерах. И нужно будет удалить когда чисто TDI диалогов не останется.
	/// </summary>
	public interface ITdiCompatibilityNavigation : INavigationManager
	{
		IPage<TViewModel> OpenViewModelOnTdi<TViewModel>(
			Tdi.ITdiTab master,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModelOnTdi<TViewModel, TCtorArg1>(
			Tdi.ITdiTab master, TCtorArg1 arg1,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModelOnTdi<TViewModel, TCtorArg1, TCtorArg2>(
			Tdi.ITdiTab master,
			TCtorArg1 arg1,
			TCtorArg2 arg2,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;
		IPage<TViewModel> OpenViewModelOnTdiTypedArgs<TViewModel>(
			Tdi.ITdiTab master,
			Type[] ctorTypes,
			object[] ctorValues,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TViewModel> configureViewModel = null,
			Action<ContainerBuilder> addingRegistrations = null) where TViewModel : DialogViewModelBase;

		ITdiPage OpenTdiTabOnTdi<TTab>(
			ITdiTab masterTab,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTabOnTdi<TTab, TCtorArg1>(
			ITdiTab masterTab,
			TCtorArg1 arg1,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTabOnTdi<TTab, TCtorArg1, TCtorArg2>(
			ITdiTab masterTab,
			TCtorArg1 arg1,
			TCtorArg2 arg2,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTabOnTdi<TTab>(
			ITdiTab masterTab,
			Type[] ctorTypes,
			object[] ctorValues,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTabOnTdiNamedArgs<TTab>(
			ITdiTab masterTab,
			IDictionary<string, object> ctorArgs,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;

		ITdiPage OpenTdiTab<TTab>(
			DialogViewModelBase master,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTab<TTab, TCtorArg1>(
			DialogViewModelBase master, TCtorArg1 arg1,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTab<TTab, TCtorArg1, TCtorArg2>(
			DialogViewModelBase master,
			TCtorArg1 arg1,
			TCtorArg2 arg2, OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTab<TTab>(
			DialogViewModelBase master,
			Type[] ctorTypes,
			object[] ctorValues,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
		ITdiPage OpenTdiTabNamedArgs<TTab>(
			DialogViewModelBase master,
			IDictionary<string, object> ctorArgs,
			OpenPageOptions options = OpenPageOptions.None,
			Action<TTab> configureTab = null,
			Action<ContainerBuilder> addingRegistrations = null) where TTab : ITdiTab;
	}
}

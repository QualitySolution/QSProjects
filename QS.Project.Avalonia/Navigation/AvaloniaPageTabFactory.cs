using Autofac;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Navigation;

public class AvaloniaPageTabFactory(ILifetimeScope container) : IViewModelsPageFactory {
	public IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(
		IDialogViewModel master,
		IDictionary<string, object> ctorArgs,
		string hash,
		Action<ContainerBuilder> addingRegistrations,
		Action<TViewModel>? configureViewModel = null)
			where TViewModel : IDialogViewModel =>
		AvaloniaPageBuilder.Create(container,
			ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value)),
			hash, addingRegistrations, configureViewModel,
			(viewModel, pageHash) => new AvaloniaPage<TViewModel>(viewModel, pageHash));

	public IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(
		IDialogViewModel master,
		Type[] ctorTypes,
		object[] ctorValues,
		string hash,
		Action<ContainerBuilder> addingRegistrations,
		Action<TViewModel>? configureViewModel = null)
			where TViewModel : IDialogViewModel =>
		AvaloniaPageBuilder.Create(container,
			ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val)),
			hash, addingRegistrations, configureViewModel,
			(viewModel, pageHash) => new AvaloniaPage<TViewModel>(viewModel, pageHash));
}

using Avalonia.Controls;
using QS.ViewModels;
using System.Collections.Generic;

namespace QS.Navigation;

public class AvaloniaRegisteredViewResolver(AvaloniaViewFactory viewFactory, IAvaloniaViewResolver nextResolver) : IAvaloniaViewResolver {
	List<TypeMatchViewResolveRule> registeredViews = [];

	#region Fluent

	/// <summary>
	/// Регистрируем соответствие между ViewModel и View.
	/// </summary>
	/// <returns>The view.</returns>
	/// <typeparam name="TViewModel">Тип ViewModel, может быть интерфейсом или базовым классом, для регистрации одного View ко многим реализациям ViewModel</typeparam>
	/// <typeparam name="TView">Тип View</typeparam>
	public AvaloniaRegisteredViewResolver RegisterView<TViewModel, TView>() where TViewModel : ViewModelBase where TView : Control {
		registeredViews.Add(new TypeMatchViewResolveRule(typeof(TViewModel), typeof(TView)));
		return this;
	}

	#endregion

	public Control Resolve(ViewModelBase viewModel, string? viewSuffix = null) {
		if (string.IsNullOrEmpty(viewSuffix) || viewSuffix == "View") {
			foreach(var rule in registeredViews) {
				if(rule.IsMatch(viewModel))
					return viewFactory.Create(rule.ViewType, viewModel);
			}
		}

		return nextResolver.Resolve(viewModel, viewSuffix);
	}
}

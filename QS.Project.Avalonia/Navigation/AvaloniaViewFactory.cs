using Avalonia.Controls;
using QS.ViewModels;
using System;

namespace QS.Navigation;

public class AvaloniaViewFactory(Func<IAvaloniaViewResolver> getGtkViewResolver)
{
	public Control Create(Type viewClass, ViewModelBase viewModel) {
		var constructorWithResolver = viewClass.GetConstructor([viewModel.GetType(), typeof(IAvaloniaViewResolver)]);
		if(constructorWithResolver != null)
			return (Control)constructorWithResolver.Invoke([viewModel, getGtkViewResolver()]);
		
		return (Control)Activator.CreateInstance(viewClass, viewModel);
	}
}

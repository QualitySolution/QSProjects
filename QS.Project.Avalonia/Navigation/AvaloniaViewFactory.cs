using Avalonia.Controls;
using System;

namespace QS.Navigation;

public class AvaloniaViewFactory(Func<IAvaloniaViewResolver> getViewResolver)
{
	public Control Create(Type viewClass, object viewModel) {
		// Пытаемся найти конструктор с (ViewModel, IAvaloniaViewResolver)
		var constructorWithResolver = viewClass.GetConstructor([viewModel.GetType(), typeof(IAvaloniaViewResolver)]);
		if(constructorWithResolver != null)
			return (Control)constructorWithResolver.Invoke([viewModel, getViewResolver()]);
		
		// Пытаемся найти конструктор с (ViewModel)
		var constructorWithViewModel = viewClass.GetConstructor([viewModel.GetType()]);
		if(constructorWithViewModel != null)
			return (Control)constructorWithViewModel.Invoke([viewModel]);
		
		// Пытаемся создать через конструктор без параметров
		var parameterlessConstructor = viewClass.GetConstructor(Type.EmptyTypes);
		if(parameterlessConstructor != null) {
			var view = (Control)parameterlessConstructor.Invoke(null);
			// Устанавливаем DataContext, если View поддерживает это
			view.DataContext = viewModel;
			return view;
		}
		
		throw new InvalidOperationException(
			$"Не удалось создать View типа '{viewClass.FullName}'. " +
			$"View должна иметь один из следующих конструкторов: " +
			$"({viewModel.GetType().Name}, IAvaloniaViewResolver), " +
			$"({viewModel.GetType().Name}), " +
			$"или конструктор без параметров.");
	}
}

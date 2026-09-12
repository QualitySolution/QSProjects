using Avalonia.Controls;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace QS.Navigation;

/// <param name="getService">Достаёт сервис из контейнера приложения или null, если такого нет</param>
// Делегатом, а не контейнером, чтобы библиотека не зависела от конкретного DI
public class AvaloniaViewFactory(Func<IAvaloniaViewResolver> getViewResolver, Func<Type, object?> getService)
{
	public Control Create(Type viewClass, object viewModel) {
		var constructor = FindConstructor(viewClass, viewModel);
		if(constructor != null)
			return Invoke(constructor, MakeArguments(viewClass, constructor, viewModel));

		var parameterless = viewClass.GetConstructor(Type.EmptyTypes)
			?? throw new InvalidOperationException(
				$"У View '{viewClass.FullName}' нет ни конструктора, первым параметром которого идёт {viewModel.GetType().Name}," +
				$" ни конструктора без параметров");

		var view = (Control)parameterless.Invoke(null);
		view.DataContext = viewModel;
		return view;
	}

	private static Control Invoke(ConstructorInfo constructor, object[] arguments) {
		try {
			return (Control)constructor.Invoke(arguments);
		}
		// рефлексия заворачивает любую ошибку конструктора в TargetInvocationException
		catch(TargetInvocationException ex) when(ex.InnerException != null) {
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			throw;
		}
	}

	/// <summary>
	/// Конструктор вью — тот, первый параметр которого принимает эту ViewModel
	/// </summary>
	private static ConstructorInfo? FindConstructor(Type viewClass, object viewModel) {
		var suitable = viewClass.GetConstructors()
			.Where(candidate => candidate.GetParameters().FirstOrDefault()?.ParameterType.IsInstanceOfType(viewModel) == true)
			.ToList();

		if(suitable.Count > 1)
			throw new InvalidOperationException($"У View '{viewClass.FullName}' сразу {suitable.Count} конструктора принимают {viewModel.GetType().Name}");

		return suitable.FirstOrDefault();
	}

	/// <summary>Первый аргумент — сама ViewModel, остальные приходят из контейнера.</summary>
	private object[] MakeArguments(Type viewClass, ConstructorInfo constructor, object viewModel) {
		var parameters = constructor.GetParameters();
		var arguments = new object[parameters.Length];
		arguments[0] = viewModel;

		for(int i = 1; i < parameters.Length; i++)
			arguments[i] = GetService(parameters[i].ParameterType)
				?? throw new InvalidOperationException(
					$"View '{viewClass.FullName}' просит {parameters[i].ParameterType.Name}, а такой сервис в контейнере не зарегистрирован.");

		return arguments;
	}

	// Резолвер вью приходит отложенно
	private object? GetService(Type serviceType) =>
		serviceType == typeof(IAvaloniaViewResolver) ? getViewResolver() : getService(serviceType);
}

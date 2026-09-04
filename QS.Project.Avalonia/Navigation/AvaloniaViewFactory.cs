using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QS.Navigation;

/// <param name="getService">Достаёт сервис из контейнера приложения или null, если такого нет.
/// Делегатом, а не контейнером, чтобы библиотека не зависела от конкретного DI.</param>
public class AvaloniaViewFactory(Func<IAvaloniaViewResolver> getViewResolver, Func<Type, object?> getService)
{
	public Control Create(Type viewClass, object viewModel) {
		foreach(var constructor in SuitableConstructors(viewClass, viewModel)) {
			var arguments = GetArgumentsOrDefault(constructor, viewModel);
			if(arguments != null)
				return (Control)constructor.Invoke(arguments);
		}

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
			$"View должна иметь конструктор, первым параметром которого идёт {viewModel.GetType().Name} " +
			$"(допускается базовый тип или интерфейс), а остальные параметры разрешаются контейнером, " +
			$"либо конструктор без параметров.");
	}

	/// <summary>
	/// ViewModel подбирается по совместимости типов
	/// </summary>
	private static IEnumerable<ConstructorInfo> SuitableConstructors(Type viewClass, object viewModel) =>
		viewClass.GetConstructors()
			.Select(constructor => new { Constructor = constructor, Parameters = constructor.GetParameters() })
			.Where(candidate => candidate.Parameters.Length > 0
				&& candidate.Parameters[0].ParameterType.IsInstanceOfType(viewModel))
			// Конструктор под конкретную ViewModel предпочитаем конструктору под интерфейс,
			// затем куда влезает больше зависимостей
			.OrderByDescending(candidate => candidate.Parameters[0].ParameterType == viewModel.GetType())
			.ThenByDescending(candidate => candidate.Parameters.Length)
			.Select(candidate => candidate.Constructor);

	/// <summary>
	/// Возвращает аргументы конструктора или null, если какой-то зависимости в контейнере нет
	/// </summary>
	private object[]? GetArgumentsOrDefault(ConstructorInfo constructor, object viewModel) {
		var parameters = constructor.GetParameters();
		var arguments = new object[parameters.Length];
		arguments[0] = viewModel;

		for(int i = 1; i < parameters.Length; i++) {
			var service = GetService(parameters[i].ParameterType);
			if(service == null)
				return null;
			arguments[i] = service;
		}

		return arguments;
	}

	// Резолвер вью приходит отложенно: он сам зависит от фабрики, и через контейнер получилась бы петля
	private object? GetService(Type serviceType) =>
		serviceType == typeof(IAvaloniaViewResolver) ? getViewResolver() : getService(serviceType);
}

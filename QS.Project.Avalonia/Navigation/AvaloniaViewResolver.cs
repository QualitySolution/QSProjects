using Avalonia.Controls;
using QS.ViewModels;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QS.Navigation;

/// <summary>
/// Для работы этого резолвера классы должны располагаться в проекте по следующим правилам:
/// {CoreNamespace}.ViewModels.{SubNamespaces}.{Name}ViewModel - для модели представления
/// {CoreNamespace}.Views.{SubNamespaces}.{Name}View - для Avalonia представления
/// 
/// Все проименованные части имени у классов должны быть одинаковые, тогда автоматический подбор сработает.
/// {CoreNamespace} - Любое корневое пространство имен, любой вложенности.
/// {SubNamespaces} - Подпапки пространств имен может быть любой вложенности указывающие на модуль или тематику класса, или отсутствовать.
/// {Name} - Непосредственно название диалога.
/// </summary>
public class AvaloniaViewResolver : IAvaloniaViewResolver {
	private readonly AvaloniaViewFactory viewFactory;
	Assembly[] lookupAssemblies;

	public AvaloniaViewResolver(AvaloniaViewFactory viewFactory, params Type[] typesInLookupAssemblies) 
		: this(viewFactory, typesInLookupAssemblies.Select(Assembly.GetAssembly).ToArray()) {
	}

	public AvaloniaViewResolver(AvaloniaViewFactory viewFactory, params Assembly[] lookupAssemblies) {
		this.lookupAssemblies = lookupAssemblies;
		this.viewFactory = viewFactory;
	}

	public Control Resolve(object viewModel, string? viewSuffix = null) {
		string suffix = viewSuffix ?? "View";
		var fullClassName = viewModel.GetType().FullName;
		var match = Regex.Matches(fullClassName, "^([a-zA-Z\\d\\.]+)\\.ViewModels(\\.[a-zA-Z\\d\\.]+)*\\.([a-zA-Z\\d]+)ViewModel$");
		if(match.Count != 1)
			throw new InvalidOperationException($"Имя класса {fullClassName} не соответствует шаблону `[CoreNamespace].ViewModels.[SubNamespaces].[Name]ViewModel`");
		var groups = match[0].Groups;
		var expectedViewName = $"{groups[1].Value}.Views{groups[2].Value}.{groups[3].Value}{suffix}";

		foreach(var assembly in lookupAssemblies) {
			Type viewClass = assembly.GetType(expectedViewName);
			if(viewClass != null) {
				return viewFactory.Create(viewClass, viewModel);
			}
		}

		return null;
	}

	public Control? Build(object? param)
	{
		if(param is ViewModelBase vm)
			return Resolve(vm);
		return null;
	}

	public bool Match(object? data)
	{
		return data is ViewModelBase;
	}
}


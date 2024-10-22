using Avalonia.Controls;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QS.Project.Navigation;

// TODO: от Gtk-шной отличается только названием класса ViewResolver'a/ViewFactory, может стоит переделать в интерфейс
public class AvaloniaClassNamesViewResolver : IAvaloniaViewResolver {

	private readonly AvaloniaViewFactory viewFactory;
	Assembly[] lookupAssemblies;

	public AvaloniaClassNamesViewResolver(AvaloniaViewFactory viewFactory, params Type[] typesInLookupAssemblies)
		: this(viewFactory, typesInLookupAssemblies.Select(Assembly.GetAssembly).ToArray()) { }

	public AvaloniaClassNamesViewResolver(AvaloniaViewFactory viewFactory, params Assembly[] lookupAssemblies) {
		this.lookupAssemblies = lookupAssemblies;
		this.viewFactory = viewFactory;
	}

	public Control Resolve(ViewModelBase viewModel) {
		var fullClassName = viewModel.GetType().FullName;
		var match = Regex.Matches(fullClassName, "^([a-zA-Z\\d\\.]+)\\.ViewModels(\\.[a-zA-Z\\d\\.]+)*\\.([a-zA-Z\\d]+)ViewModel$");
		if(match.Count != 1)
			throw new InvalidOperationException($"Имя класса {fullClassName} не соответствует шаблону `[CoreNamespace].ViewModels.[SubNamespaces].[Name]ViewModel`");
		var groups = match[0].Groups;
		var expectedViewName = $"{groups[1].Value}.Views{groups[2].Value}.{groups[3].Value}View";

		foreach(var assembly in lookupAssemblies) {
			Type viewClass = assembly.GetType(expectedViewName);
			if(viewClass != null) {
				return viewFactory.Create(viewClass, viewModel);
			}
		}

		return null;
	}
}

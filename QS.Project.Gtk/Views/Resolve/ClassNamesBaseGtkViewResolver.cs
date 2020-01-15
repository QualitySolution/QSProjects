using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Gtk;
using QS.ViewModels.Dialog;

namespace QS.Views.Resolve
{
	/// <summary>
	/// Для работы этого резолвера классы должны распологаться в проекте по следущим правилам:
	/// {CoreNamespace}.ViewModels.{SubNamespaces}.{Name}ViewModel - для модели представления
	/// {CoreNamespace}.Views.{SubNamespaces}.{Name}View - для gtk представления
	/// 
	/// Все проименованные части имени у классов должны быть одинаковые, тогда автоматический подбор сработает.
	/// {CoreNamespace} - Любое корневое пространство имен, любой вложенности.
	/// {SubNamespaces} - Подпапки постранств имен может быть любой вложенности указывающие на модуль или тематику класса или отсутствовать.
	/// {Name} - Непосредственно название диалога.
	/// </summary>
	public class ClassNamesBaseGtkViewResolver : IGtkViewResolver
	{
		Assembly[] lookupAssemblies;

		public ClassNamesBaseGtkViewResolver(params Type[] typesInlookupAssemblies) : this(typesInlookupAssemblies.Select(Assembly.GetAssembly).ToArray())
		{}

		public ClassNamesBaseGtkViewResolver(params Assembly[] lookupAssemblies)
		{
			this.lookupAssemblies = lookupAssemblies;
		}

		public Widget Resolve(DialogViewModelBase viewModel)
		{
			return Resolve((ViewModelBase)viewModel);
		}

		public Widget Resolve(ViewModelBase viewModel)
		{
			var fullClassName = viewModel.GetType().FullName;
			var match = Regex.Matches(fullClassName, "^([a-zA-Z\\.]+)\\.ViewModels(\\.[a-zA-Z\\.]+)*\\.([a-zA-Z]+)ViewModel$");
			if(match.Count != 1)
				throw new GtkViewResolveException($"Имя класса {fullClassName} не соответствует шаблону `[CoreNamespace].ViewModels.[SubNamespaces].[Name]ViewModel`");
			var groups = match[0].Groups;
			var expectedViewName = $"{groups[1].Value}.Views{groups[2].Value}.{groups[3].Value}View";

			foreach(var assambly in lookupAssemblies) {
				Type viewClass = assambly.GetType(expectedViewName);
				if(viewClass != null) {
					return (Widget)Activator.CreateInstance(viewClass, viewModel);
				}
			}

			return null;
		}
	}
}

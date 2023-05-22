using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Gtk;
using QS.ViewModels;

namespace QS.Views.Resolve
{
	/// <summary>
	/// Для работы этого резолвера классы должны располагаться в проекте по следующим правилам:
	/// {CoreNamespace}.ViewModels.{SubNamespaces}.{Name}ViewModel - для модели представления
	/// {CoreNamespace}.Views.{SubNamespaces}.{Name}View - для gtk представления
	/// 
	/// Все проименованные части имени у классов должны быть одинаковые, тогда автоматический подбор сработает.
	/// {CoreNamespace} - Любое корневое пространство имен, любой вложенности.
	/// {SubNamespaces} - Подпапки пространств имен может быть любой вложенности указывающие на модуль или тематику класса или отсутствовать.
	/// {Name} - Непосредственно название диалога.
	/// </summary>
	public class ClassNamesBaseGtkViewResolver : IGtkViewResolver
	{
		private readonly IGtkViewFactory viewFactory;
		Assembly[] lookupAssemblies;

		public ClassNamesBaseGtkViewResolver(IGtkViewFactory viewFactory, params Type[] typesInLookupAssemblies) : this(viewFactory, typesInLookupAssemblies.Select(Assembly.GetAssembly).ToArray()) {
			
		}

		public ClassNamesBaseGtkViewResolver(IGtkViewFactory viewFactory, params Assembly[] lookupAssemblies)
		{
			this.lookupAssemblies = lookupAssemblies;
			this.viewFactory = viewFactory;
		}

		public Widget Resolve(ViewModelBase viewModel)
		{
			var fullClassName = viewModel.GetType().FullName;
			var match = Regex.Matches(fullClassName, "^([a-zA-Z\\d\\.]+)\\.ViewModels(\\.[a-zA-Z\\d\\.]+)*\\.([a-zA-Z\\d]+)ViewModel$");
			if(match.Count != 1)
				throw new InvalidOperationException($"Имя класса {fullClassName} не соответствует шаблону `[CoreNamespace].ViewModels.[SubNamespaces].[Name]ViewModel`");
			var groups = match[0].Groups;
			var expectedViewName = $"{groups[1].Value}.Views{groups[2].Value}.{groups[3].Value}View";

			foreach(var assambly in lookupAssemblies) {
				Type viewClass = assambly.GetType(expectedViewName);
				if(viewClass != null) {
					return viewFactory.Create(viewClass, viewModel);
				}
			}

			return null;
		}
	}
}

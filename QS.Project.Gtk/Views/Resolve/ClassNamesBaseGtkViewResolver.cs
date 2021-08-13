using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Gtk;
using QS.ViewModels;

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
					var construstorWithResolver = viewClass.GetConstructor(new[] { viewModel.GetType(), typeof(IGtkViewResolver) });
					if(construstorWithResolver != null) {
						return (Widget)construstorWithResolver.Invoke(new object[] { viewModel, this });
					}
					return (Widget)Activator.CreateInstance(viewClass, viewModel);
				}
			}

			return null;
		}
	}
}

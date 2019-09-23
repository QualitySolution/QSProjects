using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Gtk;

namespace QS.Tdi.Gtk
{
	/// <summary>
	/// Для работы этого резолвера классы должны распологаться в проекте по следущим правилам:
	/// {CoreNamespace}.ViewModels.{SubNamespaces}.{Name}ViewModel - для модели представления
	/// {CoreNamespace}.Views.{SubNamespaces}.{Name}View - для gtk представления
	/// 
	/// Все проименованные части имени у классов должны быть одинаковые, тогда автоматический подбор сработает.
	/// {CoreNamespace} - Любое корневое пространство имен, любой вложенности.
	/// {SubNamespaces} - Подпапки постранств имен любой вложенности указывающие на модуль или тематику класса.
	/// {Name} - Непосредственно название диалога.
	/// </summary>
	public class BasedOnNameTDIResolver : DefaultTDIWidgetResolver
	{
		Assembly[] lookupAssemblies;

		public BasedOnNameTDIResolver(params Assembly[] lookupAssemblies)
		{
			this.lookupAssemblies = lookupAssemblies;
		}

		public override Widget Resolve(ITdiTab tab)
		{
			var widget = base.Resolve(tab);
			if(widget != null)
				return widget;


			var fullClassName = tab.GetType().FullName;
			var match = Regex.Matches(fullClassName, "^([a-zA-Z\\.]+)\\.ViewModels\\.([a-zA-Z\\.]+)\\.([a-zA-Z]+)ViewModel$");
			if(match.Count != 1)
				throw new InvalidOperationException($"Имя класса {fullClassName} не соответствует шаблону `[CoreNamespace].ViewModels.[SubNamespaces].[Name]ViewModel`");
			var groups = match[0].Groups;
			var expectedViewName = $"{groups[1].Value}.Views.{groups[2].Value}.{groups[3].Value}View";

			foreach(var assambly in lookupAssemblies) {
				Type viewClass = assambly.GetType(expectedViewName);
				if(viewClass != null) {
					return (Widget)Activator.CreateInstance(viewClass, tab);
				}
			}

			return null;
		}
	}
}

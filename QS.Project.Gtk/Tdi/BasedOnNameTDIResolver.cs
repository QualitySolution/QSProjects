using System.Reflection;
using Gtk;
using QS.ViewModels;
using QS.Views.Resolve;

namespace QS.Tdi
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
		ClassNamesBaseGtkViewResolver gtkViewResolver;

		public BasedOnNameTDIResolver(params Assembly[] lookupAssemblies)
		{
			this.gtkViewResolver = new ClassNamesBaseGtkViewResolver(lookupAssemblies);
		}

		public override Widget Resolve(ITdiTab tab)
		{
			var widget = base.Resolve(tab);
			if(widget != null)
				return widget;

			return gtkViewResolver.Resolve((DialogViewModelBase)tab);
		}
	}
}

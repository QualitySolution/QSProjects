using System.Reflection;
using Gtk;
using QS.ViewModels.Dialog;
using QS.Views.Resolve;

namespace QS.Tdi
{
	/// <summary>
	/// Для работы этого резолвера классы должны располагаться в проекте по следующим правилам:
	/// {CoreNamespace}.ViewModels.{SubNamespaces}.{Name}ViewModel - для модели представления
	/// {CoreNamespace}.Views.{SubNamespaces}.{Name}View - для gtk представления
	/// 
	/// Все проименованные части имени у классов должны быть одинаковые, тогда автоматический подбор сработает.
	/// {CoreNamespace} - Любое корневое пространство имен, любой вложенности.
	/// {SubNamespaces} - Подпапки пространств имен любой вложенности указывающие на модуль или тематику класса.
	/// {Name} - Непосредственно название диалога.
	/// </summary>
	public class BasedOnNameTDIResolver : DefaultTDIWidgetResolver
	{
		IGtkViewResolver gtkViewResolver;

		public BasedOnNameTDIResolver(IGtkViewResolver gtkViewResolver) {
			if(gtkViewResolver != null) this.gtkViewResolver = gtkViewResolver;
		}

		public override Widget Resolve(ITdiTab tab)
		{
			return base.Resolve(tab) ?? gtkViewResolver.Resolve((DialogViewModelBase)tab);
		}
	}
}

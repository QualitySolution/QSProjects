using System;
using Gtk;
using QS.ViewModels;

namespace QS.Views.Resolve {
	public class GtkViewFactory : IGtkViewFactory {
		private readonly Func<IGtkViewResolver> getGtkViewResolver;

		public GtkViewFactory(Func<IGtkViewResolver> getGtkViewResolver) {
			this.getGtkViewResolver = getGtkViewResolver;
		}

		public Widget Create(Type viewClass, ViewModelBase viewModel) {
			var constructorWithResolver = viewClass.GetConstructor(new[] { viewModel.GetType(), typeof(IGtkViewResolver) });
			if(constructorWithResolver != null) {
				return (Widget)constructorWithResolver.Invoke(new object[] { viewModel, getGtkViewResolver() });
			}
			return (Widget)Activator.CreateInstance(viewClass, viewModel);
		}
	}

	public interface IGtkViewFactory {
		Widget Create(Type viewClass, ViewModelBase viewModel);
	}
}

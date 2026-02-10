using System.Collections.Generic;
using Gtk;
using QS.Navigation;
using QS.ViewModels;

namespace QS.Views.Resolve
{
	public class RegisteredGtkViewResolver : IGtkViewResolver
	{
		List<TypeMatchViewResolveRule> registeredViews = new List<TypeMatchViewResolveRule>();
		private readonly IGtkViewFactory viewFactory;
		readonly IGtkViewResolver nextResolver;

		public RegisteredGtkViewResolver(IGtkViewFactory viewFactory, IGtkViewResolver nextResolver) {
			this.viewFactory = viewFactory;
			this.nextResolver = nextResolver;
		}

		#region Fluent

		/// <summary>
		/// Регистрируем соответствие между ViewModel и View.
		/// </summary>
		/// <returns>The view.</returns>
		/// <typeparam name="TViewModel">Тип ViewModel, может быть интерфейсом или базовым классом, для регистрации одного View ко многим реализациям ViewModel</typeparam>
		/// <typeparam name="TView">Тип View</typeparam>
		public RegisteredGtkViewResolver RegisterView<TViewModel, TView>()
			where TViewModel : ViewModelBase
			where TView : Widget
		{
			registeredViews.Add(new TypeMatchViewResolveRule(typeof(TViewModel), typeof(TView)));
			return this;
		}

		#endregion

		public Widget Resolve(object viewModel)
		{
			foreach(var rule in registeredViews) {
				if(rule.IsMatch(viewModel))
					return viewFactory.Create(rule.ViewType, viewModel);
			}

			return nextResolver.Resolve(viewModel);
		}
	}
}

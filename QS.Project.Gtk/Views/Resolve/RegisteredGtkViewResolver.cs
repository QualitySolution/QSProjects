using System;
using System.Collections.Generic;
using Gtk;
using QS.ViewModels.Dialog;

namespace QS.Views.Resolve
{
	public class RegisteredGtkViewResolver : IGtkViewResolver
	{
		List<ResolveRule> registeredViews = new List<ResolveRule>();
		readonly IGtkViewResolver nextResolver;

		public RegisteredGtkViewResolver(IGtkViewResolver nextResolver)
		{
			this.nextResolver = nextResolver;
		}

		#region Fluent

		/// <summary>
		/// Регистрируем соответсвие между ViewModel и View.
		/// </summary>
		/// <returns>The view.</returns>
		/// <typeparam name="TViewModel">Тип ViewModel, может быть интерфейсом или базовым классом, для регистрации одного View ко многим реализациям ViewModel</typeparam>
		/// <typeparam name="TView">Тип View</typeparam>
		public RegisteredGtkViewResolver RegisterView<TViewModel, TView>()
			where TViewModel : DialogViewModelBase
			where TView : Widget
		{
			registeredViews.Add(new ResolveRule(typeof(TViewModel), typeof(TView)));
			return this;
		}

		#endregion

		public Widget Resolve(DialogViewModelBase viewModel)
		{
			foreach(var rule in registeredViews) {
				if(rule.IsMath(viewModel))
					return rule.CreateView(viewModel);
			}

			return nextResolver.Resolve(viewModel);
		}
	}

	internal class ResolveRule
	{
		readonly Type ViewModelType;
		readonly Type ViewType;

		public ResolveRule(Type viewModel, Type view)
		{
			ViewModelType = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			ViewType = view ?? throw new ArgumentNullException(nameof(view));
		}

		public bool IsMath(DialogViewModelBase viewModel)
		{
			return ViewModelType.IsAssignableFrom(viewModel.GetType());
		}

		public Widget CreateView(DialogViewModelBase viewModel)
		{
			return (Widget)Activator.CreateInstance(ViewType, viewModel);
		}
	}
}

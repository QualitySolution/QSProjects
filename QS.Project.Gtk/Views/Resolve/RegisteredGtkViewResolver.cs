﻿using System;
using System.Collections.Generic;
using Gtk;
using QS.ViewModels;

namespace QS.Views.Resolve
{
	public class RegisteredGtkViewResolver : IGtkViewResolver
	{
		List<ResolveRule> registeredViews = new List<ResolveRule>();
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
			registeredViews.Add(new ResolveRule(typeof(TViewModel), typeof(TView)));
			return this;
		}

		#endregion

		public Widget Resolve(ViewModelBase viewModel)
		{
			foreach(var rule in registeredViews) {
				if(rule.IsMath(viewModel))
					return viewFactory.Create(rule.ViewType, viewModel);
			}

			return nextResolver.Resolve(viewModel);
		}
	}

	internal class ResolveRule
	{
		readonly Type ViewModelType;
		public readonly Type ViewType;

		public ResolveRule(Type viewModel, Type view)
		{
			ViewModelType = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			ViewType = view ?? throw new ArgumentNullException(nameof(view));
		}

		public bool IsMath(ViewModelBase viewModel)
		{
			return ViewModelType.IsAssignableFrom(viewModel.GetType());
		}
	}
}

using QS.ViewModels;
using System;

namespace QS.Navigation {
	public class TypeMatchViewResolveRule {
		readonly Type ViewModelType;
		public readonly Type ViewType;

		public TypeMatchViewResolveRule(Type viewModel, Type view) {
			ViewModelType = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			ViewType = view ?? throw new ArgumentNullException(nameof(view));
		}

		public bool IsMatch(object viewModel) {
			return ViewModelType.IsAssignableFrom(viewModel.GetType());
		}
	}
}

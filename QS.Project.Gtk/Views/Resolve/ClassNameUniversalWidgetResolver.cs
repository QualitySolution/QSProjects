using System;
using Gtk;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Dialog;

namespace QS.Views.Resolve
{
	public class ClassNameUniversalWidgetResolver : UniversalWidgetResolver
	{
		private readonly ClassNamesBaseGtkViewResolver classNamesBaseGtkViewResolver;

		public ClassNameUniversalWidgetResolver(ClassNamesBaseGtkViewResolver classNamesBaseGtkViewResolver)
		{
			this.classNamesBaseGtkViewResolver = classNamesBaseGtkViewResolver ?? throw new ArgumentNullException(nameof(classNamesBaseGtkViewResolver));
		}

		#region IWidgetResolver implementation

		public override Widget Resolve(ViewModelBase viewModel)
		{
			try {
				return base.Resolve(viewModel);
			} catch(GtkViewResolveException) {
				return ResolveBasedOnClassName(viewModel);
			}
		}

		#endregion IWidgetResolver implementation

		#region IFilterWidgetResolver implementation

		public override Widget Resolve(RepresentationModel.IJournalFilter filter)
		{
			try {
				return base.Resolve(filter);
			} catch(GtkViewResolveException) {
				if(filter is ViewModelBase) {
					return ResolveBasedOnClassName((ViewModelBase)filter);
				}
				throw;
			}
		}

		public override Widget Resolve(Project.Journal.IJournalFilter filter)
		{
			try {
				return base.Resolve(filter);
			} catch(GtkViewResolveException) {
				if(filter is ViewModelBase) {
					return ResolveBasedOnClassName((ViewModelBase)filter);
				}
				throw;
			}
		}

		#endregion IFilterWidgetResolver implementation

		#region ITDIWidgetResolver implementation

		public override Widget Resolve(ITdiTab tab)
		{
			try {
				return base.Resolve(tab);
			} catch(GtkViewResolveException) {
				if(tab is ViewModelBase) {
					return ResolveBasedOnClassName((ViewModelBase)tab);
				}
				throw;
			}
		}

		#endregion ITDIWidgetResolver implementation

		#region IGtkViewResolver implementation

		public override Widget Resolve(DialogViewModelBase viewModel)
		{
			try {
				return base.Resolve(viewModel);
			} catch(GtkViewResolveException) {
				return ResolveBasedOnClassName(viewModel);
			}
		}

		#endregion IGtkViewResolver implementation

		private Widget ResolveBasedOnClassName(ViewModelBase viewModelBase)
		{
			try {
				return classNamesBaseGtkViewResolver.Resolve(viewModelBase);
			} catch(GtkViewResolveException ex) {
				string fullClassName = viewModelBase.GetType().FullName;
				throw new GtkViewResolveException($"Не настроено сопоставление View для {fullClassName}, и имя класса {fullClassName} не соответствует шаблону `[CoreNamespace].ViewModels.[SubNamespaces].[Name]ViewModel`", ex);
			}
		}

	}
}

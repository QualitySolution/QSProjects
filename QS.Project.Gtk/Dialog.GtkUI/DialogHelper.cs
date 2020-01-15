using System;
using Gtk;
using QS.DomainModel.Entity;
using QS.Project.Journal.Search;
using QS.Project.Search.GtkUI;
using QS.Tdi;
using QS.Views.GtkUI;
using QS.Views.Resolve;
using QS.Project.Journal;
using QS.Journal.GtkUI;

namespace QS.Dialog.Gtk
{
	public static class DialogHelper
	{

		static DialogHelper()
		{
			ViewResolver = new ClassNameUniversalWidgetResolver(new ClassNamesBaseGtkViewResolver());
		}

		private static void RegisterDefaultViews()
		{
			ViewResolver.RegisterViewForBaseViewModel<JournalViewModelBase, JournalView>();
		}

		private static UniversalWidgetResolver viewResolver;
		public static UniversalWidgetResolver ViewResolver {
			get { return viewResolver; }
			set {
				if(viewResolver != value) {
					viewResolver = value;
					RegisterDefaultViews();
				}
			}
		}

		public static IEntityDialog FindParentEntityDialog(Widget child)
		{
			if(child.Parent == null)
				return null;
			else if(child.Parent is IEntityDialog)
				return child.Parent as IEntityDialog;
			else if(child.Parent.IsTopLevel)
				return null;
			else
				return FindParentEntityDialog(child.Parent);
		}

		public static ISingleUoWDialog FindParentUowDialog(Widget child)
		{
			if(child.Parent == null)
				return null;
			else if(child.Parent is ISingleUoWDialog)
				return child.Parent as ISingleUoWDialog;
			else if(child.Parent is ITabView && (child.Parent as ITabView).Tab is ISingleUoWDialog)
				return (child.Parent as ITabView).Tab as ISingleUoWDialog;
			else if(child.Parent is ITabView && (child.Parent as ITabView).Tab is ISingleUoWDialog)
				return (child.Parent as ITabView).Tab as ISingleUoWDialog;
			else if(child.Parent.IsTopLevel)
				return null;
			else
				return FindParentUowDialog(child.Parent);
		}

		public static ITdiTab FindParentTab(Widget child)
		{
			if(child.Parent.GetTab(out ITdiTab tab))
				return tab;
			else if(child.Parent.IsTopLevel)
				return null;
			else
				return FindParentTab(child.Parent);
		}

		private static bool GetTab(this Widget widget, out ITdiTab tab)
		{
			tab = null;
			if(widget is ITdiTab) {
				tab = (ITdiTab)widget;
				return true;
			}
			if(widget is ITabView tabWidget) {
				tab = tabWidget.Tab;
				return true;
			}
			return false;
		}

		public static string GenerateDialogHashName<TEntity>(int id) where TEntity : IDomainObject
		{
			return GenerateDialogHashName(typeof(TEntity), id);
		}

		public static string GenerateDialogHashName(Type clazz, int id)
		{
			if(!typeof(IDomainObject).IsAssignableFrom(clazz))
				throw new ArgumentException("Тип должен реализовывать интерфейс IDomainObject", "clazz");

			return string.Format("{0}_{1}", clazz.Name, id);
		}

		public static bool SaveBeforeSelectFromChildReference(Type savingEntity, Type childEntity)
		{
			var childNames = DomainHelper.GetSubjectNames(childEntity);
			var parrentNames = DomainHelper.GetSubjectNames(savingEntity);

			string message = String.Format("Необходимо сохранить основной объект «{0}», прежде чем выбирать «{1}» из подчинённого справочника. Сохранить?",
				parrentNames.Accusative,
				childNames.AccusativePlural
			);

			var md = new MessageDialog(null, DialogFlags.Modal,
				MessageType.Question,
				ButtonsType.YesNo,
				message);
			bool result = (ResponseType)md.Run() == ResponseType.Yes;
			md.Destroy();
			return result;
		}
	}
}

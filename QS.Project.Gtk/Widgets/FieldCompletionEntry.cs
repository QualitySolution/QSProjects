using System;
using System.ComponentModel;
using Gamma.GtkWidgets;
using Gtk;
using NLog;

namespace QS.Gtk.Widgets
{
	/// <summary>
	/// Виджет автоматически дополняет ввод ранее введенными в поле данными.
	/// </summary>
	[ToolboxItem(true)]
	public class FieldCompletionEntry : yEntry
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private ListStore completionListStore;

		public FieldCompletionEntry()
		{
		}

		private void fillAutocomplete()
		{
			logger.Info("Запрос данных для автодополнения...");
			completionListStore = new ListStore(typeof(string), typeof(object));

			IUnitOfWork localUoW;

			var dlg = OrmMain.FindMyDialog(this);

			if(dlg != null)
				localUoW = dlg.UoW;
			else
				localUoW = UnitOfWorkFactory.CreateWithoutRoot();

			if(ItemsQuery != null) {
				ItemsCriteria = ItemsQuery.DetachedCriteria.GetExecutableCriteria(localUoW.Session);
			} else {
				if(SubjectType == null) {
					logger.Warn("SubjectType = null, не возможно выполнить заполнение автокомплита.");
					return;
				}
				if(ItemsCriteria == null)
					ItemsCriteria = localUoW.Session.CreateCriteria(SubjectType);
			}

			foreach(var item in ItemsCriteria.List()) {
				completionListStore.AppendValues(
					GetObjectTitle(item),
					item
				);
			}
			entryObject.Completion.Model = completionListStore;
			logger.Debug("Получено {0} строк автодополения...", completionListStore.IterNChildren());
			//if (this.HasFocus)
			//	this.Completion.Complete ();
		}

		protected void OnEntryObjectChanged(object sender, EventArgs e)
		{
			if(entryChangedByUser && completionListStore == null)
				fillAutocomplete();
		}
	}
}

using System;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate.Criterion;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Text;

namespace QS.Project.Dialogs.GtkUI
{
	public static class EntityEditSimpleDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Запуск простого диалога редактирования справочника
		/// </summary>
		/// <returns>Возвращает экземпляр сохраненного объекта, загруженного в сессии диалога. То есть не переданный объект.
		/// Если пользователь отказался от сохранения возвращаем null.
		/// </returns>
		/// <param name="editObject">Объект для редактирования. Если null создаем новый объект.</param>
		public static object RunSimpleDialog(Window parent, System.Type objectType, object editObject)
		{
			string actionTitle = null;
			string dialogTitle = null;
			if(editObject == null) {
				var names = DomainHelper.GetSubjectNames(objectType);
				if(names != null && names.Gender != GrammaticalGender.Unknown) {
					switch(names.Gender) {
						case GrammaticalGender.Feminine:
							actionTitle = $"Простое редактирование новой {names.Genitive}";
							dialogTitle = $"Новая {names.Nominative}";
							break;
						case GrammaticalGender.Masculine:
							actionTitle = $"Простое редактирование нового {names.Genitive}";
							dialogTitle = $"Новый {names.Nominative}";
							break;
						case GrammaticalGender.Neuter:
							actionTitle = $"Простое редактирование нового {names.Genitive}";
							dialogTitle = $"Новое {names.Nominative}";
							break;
					}
				}
				else
					actionTitle = "Диалог простого редактирования";
			}
			else {
				actionTitle = $"Простое редактирование '{DomainHelper.GetObjectTilte(editObject)}'";
				var names = DomainHelper.GetSubjectNames(objectType);
				if(names != null && names.Genitive != null) {
					dialogTitle = $"Редактирование {names.Genitive}";
				}
			}
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot(actionTitle))
			{
				//Создаем объект редактирования
				object tempObject;
				if(editObject == null)
				{
					tempObject = Activator.CreateInstance (objectType);
				}
				else
				{
					if(editObject is IDomainObject)
					{
						tempObject = uow.GetById(objectType, (editObject as IDomainObject).Id);
					}
					else
					{
						throw new InvalidCastException ("У объекта переданного для запуска простого диалога, нет интерфейса IDomainObject, объект не может быть открыт.");
					}
				}

				//Создаем диалог
				Gtk.Dialog editDialog = new Gtk.Dialog(dialogTitle ?? "Редактирование", parent, Gtk.DialogFlags.Modal);
				editDialog.AddButton("Отмена", ResponseType.Cancel);
				editDialog.AddButton("Сохранить", ResponseType.Ok);
				Gtk.Table editDialogTable = new Table(1, 2, false);
				Label LableName = new Label("Название:");
				LableName.Justify = Justification.Right;
				editDialogTable.Attach(LableName, 0, 1, 0, 1);
				yEntry inputNameEntry = new yEntry();
				inputNameEntry.WidthRequest = 300;
				inputNameEntry.Binding.AddBinding(tempObject, "Name", w => w.Text).InitializeFromSource ();
				editDialogTable.Attach(inputNameEntry, 1, 2, 0, 1);
				editDialog.VBox.Add(editDialogTable);

				//Запускаем диалог
				editDialog.ShowAll();
				int result = editDialog.Run();
				if (result == (int)ResponseType.Ok) { 
					string name = (string)tempObject.GetPropertyValue ("Name");
					if (String.IsNullOrWhiteSpace (name)) {
						var att = DomainHelper.GetSubjectNames (tempObject);
						string subjectName = att != null ? att.Accusative : null;
						string msg = String.Format ("Название {0} пустое и не будет сохранено.",
							             string.IsNullOrEmpty (subjectName) ? "элемента справочника" : subjectName
						             );
						MessageDialogHelper.RunWarningDialog (msg);
						logger.Warn (msg);
						editDialog.Destroy ();
						return null;
					}
					var list = uow.Session.CreateCriteria (objectType)
						.Add (Restrictions.Eq ("Name", name))
						.Add (Restrictions.Not (Restrictions.IdEq (DomainHelper.GetId (tempObject))))
						.List ();
					if (list.Count > 0) {
						var att = DomainHelper.GetSubjectNames (tempObject);
						string subjectName = att != null ? att.Nominative.StringToTitleCase() : null;
						string msg = String.Format ("{0} с таким названием уже существует.",
							             string.IsNullOrEmpty (subjectName) ? "Элемент справочника" : subjectName
						             );
						MessageDialogHelper.RunWarningDialog (msg);
						logger.Warn (msg);
						editDialog.Destroy ();
						return list [0];
					}
					uow.TrySave (tempObject);
					uow.Commit ();
				} 
				else
					tempObject = null;
				
				editDialog.Destroy();
				return tempObject;
			}
		}

	}
}


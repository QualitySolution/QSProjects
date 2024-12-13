using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate.Criterion;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Utilities.Text;
using System;

namespace QS.Project.Dialogs.GtkUI {
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
		public static object RunSimpleDialog(Window parent, Type objectType, object editObject)
		{
			if(editObject == null)
				return RunSimpleDialog(parent, objectType, null);

			return RunSimpleDialog(parent, objectType, 
				DomainHelper.GetId(editObject), 
				DomainHelper.GetTitle(editObject));
		}

		public static object RunSimpleDialog(Window parent, Type objectType, int? id, string lastTitle = null)
		{
			string actionTitle = null;
			string dialogTitle = null;
			if(id == null) {
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
				actionTitle = lastTitle == null ? "Простое редактирование" : $"Простое редактирование '{lastTitle}'";
				var names = DomainHelper.GetSubjectNames(objectType);
				if(names != null && names.Genitive != null) {
					dialogTitle = $"Редактирование {names.Genitive}";
				}
			}
			using(IUnitOfWork uow = ServicesConfig.UnitOfWorkFactory.Create(actionTitle))
			{
				//Создаем объект редактирования
				object tempObject = id.HasValue ? uow.GetById(objectType, id.Value) : Activator.CreateInstance(objectType);

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
					uow.Save (tempObject);
					uow.Commit ();
				}
				else
				{
					if (uow.HasChanges)
					{
						var questionResult = MessageDialogHelper.RunQuestionDialog("Объект изменен. Сохранить изменения перед закрытием?");

						if (questionResult)
						{
							uow.Save(tempObject);
							uow.Commit();
						}
					}
					else
						tempObject = null;
				}

				editDialog.Destroy();
				return tempObject;
			}
		}

	}
}


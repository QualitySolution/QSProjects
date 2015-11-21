using System;
using NHibernate;
using NLog;
using Gtk;
using Gtk.DataBindings;

namespace QSOrmProject
{
	public static class OrmSimpleDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public static void RunSimpleDialog(Window parent, System.Type objectType, object editObject)
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot ())
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
						logger.Error("У объекта переданного для запуска простого диалога, нет интерфейса IDomainObject, объект не может быть открыт.");
						return;
					}
				}

				//Создаем диалог
				Dialog editDialog = new Dialog("Редактирование", parent, Gtk.DialogFlags.Modal);
				editDialog.AddButton("Отмена", ResponseType.Cancel);
				editDialog.AddButton("Сохранить", ResponseType.Ok);
				Gtk.Table editDialogTable = new Table(1, 2, false);
				Label LableName = new Label("Название:");
				LableName.Justify = Justification.Right;
				editDialogTable.Attach(LableName, 0, 1, 0, 1);
				DataEntry inputNameEntry = new DataEntry();
				inputNameEntry.WidthRequest = 300;
				editDialogTable.Attach(inputNameEntry, 1, 2, 0, 1);
				editDialog.VBox.Add(editDialogTable);

				inputNameEntry.Mappings = "Name";
				inputNameEntry.DataSource = tempObject;

				//Запускаем диалог
				editDialog.ShowAll();
				int result = editDialog.Run();
				if(result == (int)ResponseType.Ok)
				{ 
					uow.TrySave (tempObject);
					uow.Commit ();
					OrmMain.NotifyObjectUpdated(tempObject);
				}
				editDialog.Destroy();
			}
		}

	}
}


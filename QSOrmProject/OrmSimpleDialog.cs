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
			using (ISession dialogSession = OrmMain.Sessions.OpenSession())
			{
				//Создаем объект редактирования
				object tempObject;
				if(editObject == null)
				{
					System.Reflection.ConstructorInfo ci = objectType.GetConstructor(new Type[] { });
					tempObject = ci.Invoke(new object[] { });
					dialogSession.Persist (tempObject);
				}
				else
				{
					if(editObject is IDomainObject)
					{
						tempObject = dialogSession.Load(objectType, (editObject as IDomainObject).Id);
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
					dialogSession.Flush();
					OrmMain.NotifyObjectUpdated(tempObject);
				}
				editDialog.Destroy();
			}
		}

	}
}


using System;
using Gtk;
using QSProjectsLib;

namespace QSOrmProject
{
	public static class CommonDialogs
	{
		public static bool SaveBeforeCreateSlaveEntity(Type savingEntity, Type creatingEntity)
		{
			string  savingName = "НЕ УКАЗАНО", creatingName = "НЕ УКАЗАНО";
			object[] att = creatingEntity.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
			if (att.Length > 0) {
				if(!String.IsNullOrWhiteSpace ((att [0] as OrmSubjectAttribute).AllNames.Genitive))
				{
					creatingName = (att [0] as OrmSubjectAttribute).AllNames.Genitive;
				}
				else
				{
					creatingName = (att [0] as OrmSubjectAttribute).ObjectName;
				}
			}

			att = savingEntity.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
			if (att.Length > 0) {
				if(!String.IsNullOrWhiteSpace ((att [0] as OrmSubjectAttribute).AllNames.Accusative))
				{
					savingName = (att [0] as OrmSubjectAttribute).AllNames.Accusative;
				}
				else
				{
					savingName = (att [0] as OrmSubjectAttribute).ObjectName;
				}
			}

			string message = String.Format ("Перед созданием {0}, необходимо сохранить {1}. Сохранить?",
				creatingName,
				savingName
			);
			var md = new MessageDialog ( QSMain.ErrorDlgParrent, DialogFlags.Modal,
				MessageType.Question, 
				ButtonsType.YesNo,
					message);
			bool result = (ResponseType)md.Run () == ResponseType.Yes;
			md.Destroy ();
			return result;
		}
	}
}


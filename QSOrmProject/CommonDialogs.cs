using System;
using Gtk;
using QS.DomainModel.Entity;
using QSProjectsLib;

namespace QSOrmProject
{
	public static class CommonDialogs
	{
		/// <summary>
		/// Выводим диалог "Перед созданием {0}, необходимо сохранить {1}. Сохранить?"
		/// </summary>
		/// <param name="savingEntity">Для класса должен быть указан Родительский падеж(Genitive).</param>
		/// <param name="creatingEntity">Для класса должн быть указан Винительный падеж(Accusative).</param>
		public static bool SaveBeforeCreateSlaveEntity(Type savingEntity, Type creatingEntity)
		{
			string  savingName = "НЕ УКАЗАНО", creatingName = "НЕ УКАЗАНО";
			object[] att = creatingEntity.GetCustomAttributes (typeof(AppellativeAttribute), true);
			if (att.Length > 0) {
				if(!String.IsNullOrWhiteSpace ((att [0] as AppellativeAttribute).Genitive))
				{
					creatingName = (att [0] as AppellativeAttribute).Genitive;
				}
				else
				{
					creatingName = (att [0] as AppellativeAttribute).Nominative;
				}
			}

			att = savingEntity.GetCustomAttributes (typeof(AppellativeAttribute), true);
			if (att.Length > 0) {
				if(!String.IsNullOrWhiteSpace ((att [0] as AppellativeAttribute).Accusative))
				{
					savingName = (att [0] as AppellativeAttribute).Accusative;
				}
				else
				{
					savingName = (att [0] as AppellativeAttribute).Nominative;
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

		public static bool SaveBeforeSelectFromChildReference(Type savingEntity, Type childEntity)
		{
			var childNames = DomainHelper.GetSubjectNames (childEntity);
			var parrentNames = DomainHelper.GetSubjectNames (savingEntity);

			string message = String.Format ("Необходимо сохранить основной объект «{0}», прежде чем выбирать «{1}» из подчинённого справочника. Сохранить?",
				parrentNames.Accusative,
				childNames.AccusativePlural
			);

			var md = new MessageDialog ( QSMain.ErrorDlgParrent, DialogFlags.Modal,
				MessageType.Question, 
				ButtonsType.YesNo,
				message);
			bool result = (ResponseType)md.Run () == ResponseType.Yes;
			md.Destroy ();
			return result;
		}

		/// <summary>
		/// Выводит вопрос "Перед печатью {0}, необходимо сохранить изменения в {1}. Сохранить?"
		/// Диалог использует название сущьности в предложном падеже (Prepositional)
		/// </summary>
		public static bool SaveBeforePrint(Type savingEntity, string whatPrint)
		{
			string  savingName = "НЕ УКАЗАНО";

			var att = savingEntity.GetCustomAttributes (typeof(AppellativeAttribute), true);
			if (att.Length > 0) {
				if(!String.IsNullOrWhiteSpace ((att [0] as AppellativeAttribute).Prepositional))
				{
					savingName = (att [0] as AppellativeAttribute).Prepositional;
				}
				else
				{
					savingName = (att [0] as AppellativeAttribute).Nominative;
				}
			}

			string message = String.Format ("Перед печатью {0}, необходимо сохранить изменения в {1}. Сохранить?",
				whatPrint,
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


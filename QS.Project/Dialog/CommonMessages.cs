using System;
using QS.DomainModel.Entity;
using QS.Services;

namespace QS.Dialog
{
	public class CommonMessages
	{
		private readonly IInteractiveService interactive;

		public CommonMessages(IInteractiveService interactive)
		{
			this.interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
		}

		/// <summary>
		/// Выводит вопрос "Перед печатью {0}, необходимо сохранить изменения в {1}. Сохранить?"
		/// Диалог использует название сущности в предложном падеже (Prepositional)
		/// </summary>
		public bool SaveBeforePrint(Type savingEntity, string whatPrint)
		{
			string savingName = "НЕ УКАЗАНО";

			var att = savingEntity.GetCustomAttributes(typeof(AppellativeAttribute), true);
			if(att.Length > 0) {
				if(!String.IsNullOrWhiteSpace((att[0] as AppellativeAttribute).Prepositional)) {
					savingName = (att[0] as AppellativeAttribute).Prepositional;
				}
				else {
					savingName = (att[0] as AppellativeAttribute).Nominative;
				}
			}

			string message = String.Format("Перед печатью {0}, необходимо сохранить изменения в {1}. Сохранить?",
				whatPrint,
				savingName
			);

			return interactive.Question(message);
		}
	}
}

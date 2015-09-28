using System;
using System.Linq;

namespace QSOrmProject.RepresentationModel
{
	public abstract class RepresentationModelWithoutEntityBase<TNode> : RepresentationModelBase<TNode>, IRepresentationModel
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public Type ObjectType {
			get {
				throw new NotSupportedException ();
			}
		}

		/// <summary>
		/// Запрос у модели о необходимости обновления списка если объект изменился.
		/// </summary>
		/// <returns><c>true</c>, если небходимо обновлять список.</returns>
		/// <param name="updatedSubject">Обновившийся объект</param>
		protected abstract bool NeedUpdateFunc (object updatedSubject);

		/// <summary>
		/// Создает новый базовый клас и подписывается на обновления указанных типов, при этом конструкторе необходима реализация NeedUpdateFunc (object updatedSubject);
		/// </summary>
		protected RepresentationModelWithoutEntityBase (params Type[] subcribeOnTypes)
		{
			foreach (var type in subcribeOnTypes) {
				var map = OrmMain.GetObjectDescription (type);
				if (map != null)
					map.ObjectUpdated += OnExternalUpdateCommon;
				else
					logger.Warn ("Невозможно подписаться на обновления класа {0}. Не найден класс маппинга.", type);
			}
		}

		void OnExternalUpdateCommon (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			if (!UoW.IsAlive)
			{
				logger.Warn ("Получена нотификация о внешнем обновлении данные в {0}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.",
					this);
				return;
			}

			if (e.UpdatedSubjects.Any (NeedUpdateFunc))
				UpdateNodes ();
		}
	}
}


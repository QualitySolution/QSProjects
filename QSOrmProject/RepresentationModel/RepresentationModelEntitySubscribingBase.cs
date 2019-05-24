using System;
using System.Linq;

namespace QSOrmProject.RepresentationModel
{
	/// <summary>
	/// Базовый клас презентационной модели с подпиской на обновления для типа TEntity и любых дополнительных сущностей указанных в конструкторе.
	/// </summary>
	public abstract class RepresentationModelEntitySubscribingBase<TEntity, TNode> : RepresentationModelEntityBase<TEntity, TNode>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private Type[] subcribeOnTypes;
		/// <summary>
		/// Запрос у модели о необходимости обновления списка если объект изменился.
		/// </summary>
		/// <returns><c>true</c>, если небходимо обновлять список.</returns>
		/// <param name="updatedSubject">Обновившийся объект</param>
		protected abstract bool NeedUpdateFunc (object updatedSubject);

		/// <summary>
		/// Создает новый базовый клас и подписывается на обновления указанных типов, при этом конструкторе необходима реализация NeedUpdateFunc (object updatedSubject);
		/// </summary>
		protected RepresentationModelEntitySubscribingBase (params Type[] subcribeOnTypes)
		{
			this.subcribeOnTypes = subcribeOnTypes;
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
				throw new InvalidOperationException ($"Получена нотификация о внешнем обновлении данные в {this}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.");

			if (e.UpdatedSubjects.Any (NeedUpdateFunc))
				UpdateNodes ();
		}

		public void Destroy()
		{
			logger.Debug("{0} called Destroy()", this.GetType());
			base.Destroy();
			foreach (var type in subcribeOnTypes)
			{
				var map = OrmMain.GetObjectDescription(type);
				if (map != null)
					map.ObjectUpdated -= OnExternalUpdateCommon;
			}
		}
	}
}
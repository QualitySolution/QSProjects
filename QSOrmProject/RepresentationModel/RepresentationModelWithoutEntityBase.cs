using System;
using System.Collections.Generic;
using System.Linq;
using QS.RepresentationModel;
using QS.RepresentationModel.GtkUI;

namespace QSOrmProject.RepresentationModel
{
	/// <summary>
	/// Базовый клас презентационной модели без конкретной сущности с подпиской на обновления любых сущностей указанных в конструкторе.
	/// </summary>
	public abstract class RepresentationModelWithoutEntityBase<TNode> : RepresentationModelBase<TNode>, IRepresentationModel, QS.RepresentationModel.GtkUI.IRepresentationModel
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public Type ObjectType {
			get {
				return null;
			}
		}

		public Type EntityType => null;

		public IJournalFilter JournalFilter => RepresentationFilter as IJournalFilter;

		public virtual IEnumerable<IJournalPopupItem> PopupItems => new List<IJournalPopupItem>();

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
		protected RepresentationModelWithoutEntityBase (params Type[] subcribeOnTypes)
		{
			this.subcribeOnTypes = subcribeOnTypes;
			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribeOnEntity(HandleEntityChangeEvent, subcribeOnTypes);
		}

		void HandleEntityChangeEvent(QS.DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			if(!UoW.IsAlive) {
				logger.Warn("Получена нотификация о внешнем обновлении данные в {0}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.",
					this);
				return;
			}

			if(changeEvents.Select(x => x.Entity).Any(NeedUpdateFunc))
				UpdateNodes();
		}

		public void Destroy()
		{
			logger.Debug("{0} called Destroy()", this.GetType());
			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.UnsubscribeAll(this);
		}
	}
}


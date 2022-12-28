using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.NotifyChange;
using QS.RepresentationModel;
using QS.RepresentationModel.GtkUI;

namespace QSOrmProject.RepresentationModel
{
	/// <summary>
	/// Базовый класс презентационной модели с подпиской на обновления только для типа TEntity
	/// </summary>
	public abstract class RepresentationModelEntityBase<TEntity, TNode> : RepresentationModelBase<TNode>, IRepresentationModel, QS.RepresentationModel.GtkUI.IRepresentationModel
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public Type ObjectType => typeof(TEntity);

		public Type EntityType => ObjectType;

		public IJournalFilter JournalFilter => RepresentationFilter as IJournalFilter;

		public virtual IEnumerable<IJournalPopupItem> PopupItems => new List<IJournalPopupItem>();

		/// <summary>
		/// Запрос у модели о необходимости обновления списка если объект изменился.
		/// </summary>
		/// <returns><c>true</c>, если необходимо обновлять список.</returns>
		/// <param name="updatedSubject">Обновившийся объект</param>
		protected abstract bool NeedUpdateFunc(TEntity updatedSubject);

		/// <summary>
		/// Создает новый базовый класс и подписывается на обновления для типа TEntity, при этом конструкторе необходима реализация NeedUpdateFunc (TEntity updatedSubject)
		/// </summary>
		protected RepresentationModelEntityBase()
		{
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<TEntity>(OnExternalUpdate);
		}

		void OnExternalUpdate(EntityChangeEvent[] changeEvents)
		{
			if(!UoW.IsAlive) {
				logger.Warn($"Получена нотификация о внешнем обновлении данные в {this}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.");
				return;
			}

			if(changeEvents.Select(x => x.Entity).Cast<TEntity>().Any(NeedUpdateFunc))
				UpdateNodes();
		}

		public void Destroy()
		{
			Dispose();
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			logger.Debug("{0} called Destroy()", this.GetType());
		}
	}
}

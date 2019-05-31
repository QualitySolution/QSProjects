using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.NotifyChange;
using QS.RepresentationModel;
using QS.RepresentationModel.GtkUI;

namespace QSOrmProject.RepresentationModel
{
	/// <summary>
	/// Базовый клас презентационной модели с подпиской на обновления только для типа TEntity
	/// </summary>
	public abstract class RepresentationModelEntityBase<TEntity, TNode> : RepresentationModelBase<TNode>, IRepresentationModel, QS.RepresentationModel.GtkUI.IRepresentationModel
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public Type ObjectType {
			get { return typeof(TEntity); }
		}

		public Type EntityType => ObjectType;

		public IJournalFilter JournalFilter => RepresentationFilter as IJournalFilter;

		public virtual IEnumerable<IJournalPopupItem> PopupItems => new List<IJournalPopupItem>();

		/// <summary>
		/// Запрос у модели о необходимости обновления списка если объект изменился.
		/// </summary>
		/// <returns><c>true</c>, если небходимо обновлять список.</returns>
		/// <param name="updatedSubject">Обновившийся объект</param>
		protected abstract bool NeedUpdateFunc (TEntity updatedSubject);

		/// <summary>
		/// Создает новый базовый клас и подписывается на обновления для типа TEntity, при этом конструкторе необходима реализация NeedUpdateFunc (TEntity updatedSubject)
		/// </summary>
		protected RepresentationModelEntityBase ()
		{
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<TEntity>(OnExternalUpdate);
		}

		void OnExternalUpdate (EntityChangeEvent[] changeEvents)
		{
			if (!UoW.IsAlive)
				throw new InvalidOperationException($"Получена нотификация о внешнем обновлении данные в {this}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.");

			if(changeEvents.Select(x => x.Entity).Cast<TEntity>().Any(NeedUpdateFunc))
				UpdateNodes();
		}

		public void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			logger.Debug("{0} called Destroy()", this.GetType());
		}
	}
}
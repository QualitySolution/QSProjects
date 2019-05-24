using System;
using System.Linq;
using QS.DomainModel.NotifyChange;

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
			NotifyConfiguration.Instance.BatchSubscribeOnEntity(OnExternalUpdateCommon, subcribeOnTypes);
		}

		void OnExternalUpdateCommon (EntityChangeEvent[] changeEvents)
		{
			if (!UoW.IsAlive)
				throw new InvalidOperationException ($"Получена нотификация о внешнем обновлении данные в {this}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.");

			if (changeEvents.Select(x => x.Entity).Any(NeedUpdateFunc))
				UpdateNodes ();
		}

		public new void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			logger.Debug("{0} called Destroy()", this.GetType());
			base.Destroy();
		}
	}
}
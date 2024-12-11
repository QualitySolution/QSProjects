﻿using System;
using System.Linq;
using QS.DomainModel.NotifyChange;

namespace QSOrmProject.RepresentationModel
{
	/// <summary>
	/// Базовый класс презентационной модели с подпиской на обновления для типа TEntity и любых дополнительных сущностей указанных в конструкторе.
	/// </summary>
	public abstract class RepresentationModelEntitySubscribingBase<TEntity, TNode> : RepresentationModelEntityBase<TEntity, TNode>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private Type[] subcribeOnTypes;
		/// <summary>
		/// Запрос у модели о необходимости обновления списка если объект изменился.
		/// </summary>
		/// <returns><c>true</c>, если необходимо обновлять список.</returns>
		/// <param name="updatedSubject">Обновившийся объект</param>
		protected abstract bool NeedUpdateFunc (object updatedSubject);

		/// <summary>
		/// Создает новый базовый класс и подписывается на обновления указанных типов, при этом конструкторе необходима реализация NeedUpdateFunc (object updatedSubject);
		/// </summary>
		protected RepresentationModelEntitySubscribingBase (params Type[] subcribeOnTypes)
		{
			this.subcribeOnTypes = subcribeOnTypes;
			//FIXME Если кто-то будет использовать необходимо переделать на DI
			//NotifyConfiguration.Instance.BatchSubscribeOnEntity(OnExternalUpdateCommon, subcribeOnTypes);
		}

		void OnExternalUpdateCommon (EntityChangeEvent[] changeEvents)
		{
			if (!UoW.IsAlive) {
				logger.Warn($"Получена нотификация о внешнем обновлении данные в {this}, в тот момент когда сессия уже закрыта. Возможно RepresentationModel, осталась в памяти при закрытой сессии.");
				return;
			}

			if (changeEvents.Select(x => x.Entity).Any(NeedUpdateFunc))
				UpdateNodes ();
		}

		public new void Destroy()
		{
			//FIXME Если кто-то будет использовать необходимо переделать на DI
			//NotifyConfiguration.Instance.UnsubscribeAll(this);
			logger.Debug("{0} called Destroy()", this.GetType());
			base.Destroy();
		}
	}
}

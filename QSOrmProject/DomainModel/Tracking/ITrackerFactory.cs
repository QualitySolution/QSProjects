using System;
using QSOrmProject;

namespace QS.DomainModel.Tracking
{
	public interface ITrackerFactory
	{
		/// <summary>
		/// Создаем трекер для указанного объекта. 
		/// </summary>
		/// <returns>Трекер или null если в создании трекера нет необходимости.</returns>
		/// <param name="root">Корневой объект для которого создается трекер</param>
		/// <typeparam name="TEntity">Тип объекта</typeparam>
		IObjectTracker<TEntity> Create<TEntity>(TEntity root, TrackerCreateOption option) where TEntity : class, IDomainObject, new();
		IObjectTracker CreateTracker(object root, TrackerCreateOption option);

		IDeleteTracker CreateDeleteTracker();

		bool NeedTrace(Type type);
	}

	public enum TrackerCreateOption
	{
		/// <summary>
		/// Это новый объект. Первый снимок будет взять с текущего состояния объекта.
		/// </summary>
		IsNewAndShotThis,
		/// <summary>
		/// Это загруженный объект. Первый снимок будет взять с текущего состояния объекта.
		/// </summary>
		IsLoadedAndShotThis,
		/// <summary>
		/// Это новый объект. Первый снимок будет взять с пустого состояния объекта.
		/// </summary>
		IsNewAndShotEmpty
	}
}

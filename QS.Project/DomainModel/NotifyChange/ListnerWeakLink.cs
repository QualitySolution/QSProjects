using System;
using System.Linq;
using System.Reflection;

namespace QS.DomainModel.NotifyChange
{
	public delegate void SingleEntityChangeEventMethod(EntityChangeEvent changeEvent);
	public delegate void ManyEntityChangeEventMethod(EntityChangeEvent[] changeEvents);

	public enum NotifyMode
	{
		Single,
		Many
	}

	public class SubscriberWeakLink
	{
		WeakReference targetReference;
		MethodInfo method;
		NotifyMode mode;

		internal object Owner => targetReference.Target;

		internal Type[] EntityTypes { get; private set; }

		internal SubscriberWeakLink(Type entityClass, SingleEntityChangeEventMethod handler)
		{
			targetReference = new WeakReference(handler.Target);
			method = handler.Method;
			mode = NotifyMode.Single;

			EntityTypes = new[] { entityClass };
		}

		internal SubscriberWeakLink(Type[] entityClasses, ManyEntityChangeEventMethod handler)
		{
			targetReference = new WeakReference(handler.Target);
			method = handler.Method;
			mode = NotifyMode.Many;
			EntityTypes = entityClasses;
		}

		internal bool IsAlive => targetReference != null && targetReference.IsAlive;

		internal bool Invoke(EntityChangeEvent[] changeEvents)
		{
			if (!IsAlive) return false;

			if (mode != NotifyMode.Many)
				throw new InvalidOperationException("Переданный метод должен реализовать режим Many");

			if (targetReference.Target != null)
				method.Invoke(targetReference.Target, new object[] {changeEvents });

			return true;
		}

		internal bool Invoke(EntityChangeEvent changeEvent)
		{
			if (!IsAlive) return false;

			if (mode != NotifyMode.Single)
				throw new InvalidOperationException("Переданный метод должен реализовать режим Single");

			if (targetReference.Target != null)
				method.Invoke(targetReference.Target, new object[] { changeEvent });

			return true;
		}

		internal bool IsSuitable(EntityChangeEvent changeEvent)
		{
			return EntityTypes.Contains(changeEvent.EntityClass);
		}

		public override string ToString()
		{
			return $"{method.DeclaringType}.{method.Name}";
		}

		#region FluentFilter

		#endregion
	}
}

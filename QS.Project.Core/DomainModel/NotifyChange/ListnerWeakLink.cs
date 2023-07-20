using System;
using System.Linq;
using System.Reflection;
using QS.DomainModel.NotifyChange.Conditions;

namespace QS.DomainModel.NotifyChange
{
	public delegate void BatchEntityChangeHandler(EntityChangeEvent[] changeEvents);

	public class SubscriberWeakLink
	{
		WeakReference targetReference;
		MethodInfo method;
		readonly SelectionConditions conditions;

		internal object Owner => targetReference.Target;

		internal Type[] EntityTypes { get; private set; }

		#region Конструкторы

		internal SubscriberWeakLink(Type[] entityClasses, BatchEntityChangeHandler handler)
		{
			ParseHandler(handler);
			EntityTypes = entityClasses;
		}

		internal SubscriberWeakLink(SelectionConditions conditions, BatchEntityChangeHandler handler)
		{
			ParseHandler(handler);
			this.conditions = conditions;
		}

		internal SubscriberWeakLink(BatchEntityChangeHandler handler)
		{
			ParseHandler(handler);
			EntityTypes = new Type[] { };
		}

		private void ParseHandler(BatchEntityChangeHandler handler)
		{
			targetReference = new WeakReference(handler.Target);
			method = handler.Method;
		}

		#endregion

		//Подписчики являющиеся статическими методами всегда живы.
		internal bool IsAlive => method.IsStatic || (targetReference != null && targetReference.IsAlive);

		internal bool Invoke(EntityChangeEvent[] changeEvents)
		{
			if (!IsAlive) return false;

			method.Invoke(targetReference.Target, new object[] {changeEvents });
			return true;
		}

		internal bool IsSuitable(EntityChangeEvent changeEvent)
		{
			if (conditions != null)
				return conditions.IsSuitable(changeEvent);

			if (EntityTypes.Length == 0)
				return true;
			return EntityTypes.Contains(changeEvent.EntityClass);
		}

		public override string ToString()
		{
			return $"{method.DeclaringType}.{method.Name}";
		}
	}
}

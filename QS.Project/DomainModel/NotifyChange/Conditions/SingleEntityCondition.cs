using System;
namespace QS.DomainModel.NotifyChange.Conditions
{
	public class SingleEntityCondition<TEntity> : ICondition
	{
		readonly SelectionConditions myConditionsGroup;

		public SingleEntityCondition(SelectionConditions myConditionsGroup)
		{
			this.myConditionsGroup = myConditionsGroup;
		}

		#region Fluent Conditions

		TypeOfChangeEvent? typeOfChange;

		public SingleEntityCondition<TEntity> AndChangeType(TypeOfChangeEvent typeOfChange)
		{
			this.typeOfChange = typeOfChange;
			return this;
		}

		#endregion

		public bool IsSuitable(EntityChangeEvent changeEvent)
		{
			if (changeEvent.EntityClass != typeof(TEntity))
				return false;

			if (typeOfChange.HasValue && typeOfChange.Value != changeEvent.EventType)
				return false;

			return true;
		}
	}
}

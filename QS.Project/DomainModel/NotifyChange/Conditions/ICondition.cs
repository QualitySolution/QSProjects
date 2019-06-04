using System;
namespace QS.DomainModel.NotifyChange.Conditions
{
	public interface ICondition
	{
		bool IsSuitable(EntityChangeEvent changeEvent);
	}
}

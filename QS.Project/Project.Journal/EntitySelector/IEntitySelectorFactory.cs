using System;
namespace QS.Project.Journal.EntitySelector
{
	public interface IEntitySelectorFactory
	{
		Type EntityType { get; }
		IEntitySelector CreateSelector(bool multipleSelect = false);
	}
}

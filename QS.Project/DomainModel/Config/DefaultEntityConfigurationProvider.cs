using System;
namespace QS.DomainModel.Config
{
	public class DefaultEntityConfigurationProvider : IEntityConfigurationProvider
	{
		public IEntityConfig GetEntityConfig(Type entityType)
		{
			return DomainConfiguration.GetEntityConfig(entityType);
		}
	}
}

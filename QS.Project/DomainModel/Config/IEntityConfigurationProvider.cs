using System;
namespace QS.DomainModel.Config
{
	public interface IEntityConfigurationProvider
	{
		IEntityConfig GetEntityConfig(Type entityType);
	}
}

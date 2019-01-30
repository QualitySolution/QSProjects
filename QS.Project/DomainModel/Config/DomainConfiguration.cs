using System;
namespace QS.DomainModel.Config
{
	public static class DomainConfiguration
	{
		public static Func<Type, IEntityConfig> GetEntityConfig;
	}
}

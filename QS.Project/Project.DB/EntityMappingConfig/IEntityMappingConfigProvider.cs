namespace QS.Project.DB.EntityMappingConfig
{
	public interface IEntityMappingConfigProvider
	{
		IEntityMappingConfig GetEntityMappingConfig<T>() where T : class;
	}
}

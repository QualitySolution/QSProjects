namespace QS.MachineConfig.Configuration
{
	public interface IConfigurationManager
	{
		IAppConfig GetConfiguration();
		void SaveConfigration(IAppConfig appConfig);
	}
}

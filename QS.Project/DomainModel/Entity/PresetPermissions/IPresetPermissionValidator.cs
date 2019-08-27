namespace QS.DomainModel.Entity.PresetPermissions
{
	public interface IPresetPermissionValidator
	{
		bool Validate(string presetPermissionName, int userId);
	}
}
using System;
using QS.DomainModel.Entity.PresetPermissions;

namespace QS.Permissions
{
	//FIXME Временная заглушка для валидатора прав, пока не появится возмоность его просто не настраивать, в тех случаях когда права не используются.
	public class EverythingAllowedPresetPermissionValidator : IPresetPermissionValidator
	{
		public bool Validate(string presetPermissionName, int userId)
		{
			return true;
		}
	}
}

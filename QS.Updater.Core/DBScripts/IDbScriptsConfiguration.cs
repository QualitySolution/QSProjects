using QS.DBScripts.Models;
using QS.Updater.DB;

namespace QS.DBScripts
{
	public interface IDbScriptsConfiguration
	{
		CreationScript MakeCreationScript();

		UpdateConfiguration MakeUpdateConfiguration();
	}
}

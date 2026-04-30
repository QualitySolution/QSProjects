using System;

namespace QS.DBScripts.Controllers
{
	/// <summary>
	/// Источник SQL-скриптов конкретного приложения, передаваемый в реализации IDBCreator
	/// </summary>
	public interface IDbScriptsConfiguration
	{
		Version CreationVersion { get; }

		string GetCreationSqlScript();
	}
}

using System;
namespace QS.Updater
{
	public interface ISkipVersionState
	{
		bool IsSkipedVersion(string version);
		void SaveSkipVersion(string version);
	}
}

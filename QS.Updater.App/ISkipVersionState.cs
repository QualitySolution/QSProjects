using System;
namespace QS.Updater
{
	public interface ISkipVersionState
	{
		bool IsSkippedVersion(Version version);
		void SaveSkipVersion(Version version);
	}
}

using System;
namespace QS.Updater.App
{
	public interface ISkipVersionState
	{
		bool IsSkippedVersion(Version version);
		void SaveSkipVersion(Version version);
	}
}

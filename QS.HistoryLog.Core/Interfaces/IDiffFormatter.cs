using System;
namespace QS.HistoryLog.Core
{
	public interface IDiffFormatter
	{
		void SideBySideDiff(string oldValue, string newValue, out string oldDiff, out string newDiff);
	}
}

using System;
namespace QS.HistoryLog
{
	public interface IDiffFormatter
	{
		void SideBySideDiff(string oldValue, string newValue, out string oldDiff, out string newDiff);
	}
}

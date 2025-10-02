using System;
namespace QS.DomainModel.Entity 
{
	public interface IDiffFormatter
	{
		void SideBySideDiff(string oldValue, string newValue, out string oldDiff, out string newDiff);
	}
}

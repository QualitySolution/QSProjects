using System;

namespace QSTDI
{
	public interface ITdiTabParent
	{
		void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab);
	}
}


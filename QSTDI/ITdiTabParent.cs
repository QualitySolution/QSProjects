using System;

namespace QSTDI
{
	public interface ITdiTabParent
	{
		void AddSlaveTab(ITdiTab masterTab, ITdiTab slaveTab);
		void AddTab(ITdiTab tab, ITdiTab afterTab);
		ITdiDialog OnCreateDialogWidget(TdiOpenObjDialogEventArgs eventArgs);
	}
}


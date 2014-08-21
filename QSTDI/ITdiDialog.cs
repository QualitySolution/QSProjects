using System;

namespace QSTDI
{
	public interface ITdiDialog : ITdiTab
	{
		bool HasChanges { get;}
		bool Save();
	}
}


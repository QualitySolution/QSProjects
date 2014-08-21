using System;

namespace QSTDI
{
	public interface ITdiTab
	{
		string TabName { set; get;}
		event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
	}

	public class TdiTabNameChangedEventArgs : EventArgs
	{
		public string NewName { get; private set; }

		public TdiTabNameChangedEventArgs(string newName)
		{
			NewName = newName;
		}
	}
}


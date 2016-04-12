using System;

namespace QSTDI
{
	public interface ITdiTab
	{
		string TabName { get;}
		ITdiTabParent TabParent { set; get;}
		event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		event EventHandler<TdiTabCloseEventArgs> CloseTab;
		bool FailInitialize { get;}

		bool CompareHashName(string hashName);
	}

	public class TdiTabNameChangedEventArgs : EventArgs
	{
		public string NewName { get; private set; }

		public TdiTabNameChangedEventArgs(string newName)
		{
			NewName = newName;
		}
	}

	public class TdiTabCloseEventArgs : EventArgs
	{
		public bool AskSave { get; private set; }

		public TdiTabCloseEventArgs(bool askSave)
		{
			AskSave = askSave;
		}
	}

}


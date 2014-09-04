using System;

namespace QSTDI
{
	public interface ITdiJournal : ITdiTab
	{
		event EventHandler<TdiOpenObjDialogEventArgs> OpenObjDialog;
		event EventHandler<TdiOpenObjDialogEventArgs> DeleteObj;
	}

	public class TdiOpenObjDialogEventArgs : EventArgs
	{
		public System.Type ObjectClass { get; private set; }
		public object ObjectVar{get; set;}
		public int ObjectId { get; set; }
		public bool NewObject { get; private set; }
		public ITdiDialog ResultDialogWidget { get; set; }

		public TdiOpenObjDialogEventArgs(System.Type objectClass, int objectId)
		{
			NewObject = false;
			ObjectClass = objectClass;
			ObjectId = objectId;
		}

		public TdiOpenObjDialogEventArgs(System.Type objectClass)
		{
			NewObject = true;
			ObjectClass = objectClass;
		}

		public TdiOpenObjDialogEventArgs(object objectVar)
		{
			NewObject = false;
			ObjectVar = objectVar;
			ObjectClass = objectVar.GetType();
		}

	}
}


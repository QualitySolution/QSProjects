using System;
using Gtk;

namespace Gamma.GtkHelpers
{
	public static class ListStoreHelper
	{
		public static bool SearchListStore( ListStore list, object searchvalue, int column, out TreeIter iter)
		{   
			if(list.GetIterFirst(out iter))
			{
				if( searchvalue.Equals (list.GetValue(iter, column)))
					return true;
			}
			else
				return false;
			while (list.IterNext(ref iter)) 
			{
				if( searchvalue.Equals (list.GetValue(iter, column)))
					return true;
			}
			return false;		
		}

		public static bool SearchListStore<TObject>(ListStore list, Func<TObject, bool> searchFunc, int column, out TreeIter iter)
		{   
			if(!list.GetIterFirst(out iter))
				return false;
			do {
				object item = list.GetValue (iter, column);

				if(item is TObject && searchFunc((TObject)item))
					return true;
			} while (list.IterNext (ref iter));
			return false;
		}
	}
}


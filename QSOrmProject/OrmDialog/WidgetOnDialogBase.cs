using System;
using QSTDI;

namespace QSOrmProject
{
	/// <summary>
	/// Используется для удобного доступа к родительскому диалогу и вкладке.
	/// </summary>
	public class WidgetOnDialogBase : WidgetOnTdiTabBase
	{
		IOrmDialog myOrmDialog;

		protected IOrmDialog MyOrmDialog{
			get{
				if(myOrmDialog == null)
					myOrmDialog = OrmMain.FindMyDialog (this);
				if (myOrmDialog == null) {
					throw new InvalidOperationException ("Родительский диалог не найден.");
				} else
					return myOrmDialog;
			}
		}

		public ITdiDialog MyTdiDialog{
			get {
				var dlg = MyTab as ITdiDialog;
				if(dlg == null)
					throw new InvalidOperationException ("Родительская вкладка не является диалогом.");
				return dlg;
			}
		}

		protected override void OnParentSet (Gtk.Widget previous_parent)
		{
			myOrmDialog = null;
			base.OnParentSet (previous_parent);
		}
	}
}


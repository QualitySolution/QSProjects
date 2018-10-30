using System;
using QS.Project.Dialogs;
using QS.Project.Dialogs.Gtk;
using QSTDI;

namespace QSOrmProject
{
	/// <summary>
	/// Используется для удобного доступа к родительскому диалогу и вкладке.
	/// </summary>
	public class WidgetOnDialogBase : WidgetOnTdiTabBase
	{
		IEntityDialog myOrmDialog;

		protected IEntityDialog MyOrmDialog{
			get{
				if(myOrmDialog == null)
					myOrmDialog = DialogHelper.FindParentDialog (this);
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


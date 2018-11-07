using System;
using Gtk;
using QS.Tdi;

namespace QS.Dialog.Gtk
{
	/// <summary>
	/// Используется для удобного доступа к родительскому диалогу и вкладке.
	/// </summary>
	public class WidgetOnDialogBase : WidgetOnTdiTabBase
	{
		ISingleUoWDialog myOrmDialog;

		protected ISingleUoWDialog MyOrmDialog{
			get{
				if(myOrmDialog == null)
					myOrmDialog = DialogHelper.FindParentUowDialog(this);
				if (myOrmDialog == null) {
					throw new InvalidOperationException ("Родительский диалог не найден.");
				} else
					return myOrmDialog;
			}
		}

		IEntityDialog myEntityDialog;

		protected IEntityDialog MyEntityDialog {
			get {
				if(myEntityDialog == null)
					myEntityDialog = DialogHelper.FindParentEntityDialog(this);
				if(myEntityDialog == null) {
					throw new InvalidOperationException("Родительский диалог не найден.");
				} else
					return myEntityDialog;
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

		protected override void OnParentSet (Widget previous_parent)
		{
			myOrmDialog = null;
			base.OnParentSet (previous_parent);
		}
	}
}


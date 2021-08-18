using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using Gamma.Utilities;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yEntry : Entry
	{
		public BindingControler<yEntry> Binding { get; private set;}

		public yEntry ()
		{
			Binding = new BindingControler<yEntry> (this, new Expression<Func<yEntry, object>>[] {
				(w => w.Text)
			});
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.Text);
			base.OnChanged ();
		}

		#region Colors

		private string textColor;
		public string TextColor {
			get => textColor;
			set {
				textColor = value;
				ModifyText(StateType.Normal, ColorUtil.Create(value));
			}
		}

		#endregion
	}
}


using System;
using Gtk;
using Gamma.Binding.Core;
using Gamma.Utilities;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yLabel : Label
	{
		public BindingControler<yLabel> Binding { get; private set;}

		public yLabel ()
		{
			Binding = new BindingControler<yLabel> (this);
		}

		private string foregroundColor;
		public string ForegroundColor {
			get => foregroundColor;
			set {
				foregroundColor = value;
				if(String.IsNullOrEmpty(value))
					ModifyFg(StateType.Normal);
				else {
					ModifyFg(StateType.Normal, ColorUtil.Create(value));
				}
			}
		}
	}
}


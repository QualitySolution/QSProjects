using System;
using Gtk;
using Gamma.Binding.Core;
using Gamma.Utilities;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yEventBox : EventBox
	{
		public BindingControler<yEventBox> Binding { get; private set;}

		public yEventBox()
		{
			Binding = new BindingControler<yEventBox> (this);
		}

		#region Colors

		private string backgroundColor;
		public string BackgroundColor {
			get => backgroundColor;
			set {
				backgroundColor = value;
				ModifyBg(StateType.Normal, ColorUtil.Create(value));
			}
		}

		#endregion
	}
}


using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using Gamma.Utilities;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yRadioButton : RadioButton
	{
		public BindingControler<yRadioButton> Binding { get; private set; }

		public yRadioButton(string label = "") : base(label)
		{
			Binding = new BindingControler<yRadioButton> (this, new Expression<Func<yRadioButton, object>>[] {
				(w => w.Active),
				(w => w.BindedValue)
			});

			Toggled += YRadioButton_Toggled;
		}


		public yRadioButton(object tag) : this()
		{
			Tag = tag;
		}

		/// <summary>
		/// For store user data, like WinForms Tag.
		/// </summary>
		public object Tag;

		void YRadioButton_Toggled(object sender, EventArgs e)
		{
			Console.WriteLine($"togleed {BindValueWhenActivated} = {Active}");
			Binding.FireChange(w => w.Active);
			if (Active)
				Binding.FireChange(w => w.BindedValue);
		}

		public object BindValueWhenActivated { get; set; }

		public object BindedValue
		{
			get => Active ? BindValueWhenActivated : null;
			set => Active = TypeUtil.EqualBoxedValues(value, BindValueWhenActivated);
		}
	}
}

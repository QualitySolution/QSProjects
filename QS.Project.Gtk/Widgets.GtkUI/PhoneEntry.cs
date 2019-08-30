using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QS.Utilities.Numeric;

namespace QS.Widgets.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class PhoneEntry : Gtk.Entry
	{
		public BindingControler<PhoneEntry> Binding { get; private set; }

		/// <summary>
		/// Для хранения пользовательской информации как в WinForms
		/// </summary>
		public object Tag;

		private PhoneFormatter phoneFormatter = new PhoneFormatter();

		public PhoneFormat PhoneFormat
		{
			get => phoneFormatter.Format;
			set => phoneFormatter.Format = value;
		}

		public PhoneEntry() 
		{
			Binding = new BindingControler<PhoneEntry>(this, new Expression<Func<PhoneEntry, object>>[] {
				(w => w.Text)
			});

			PhoneFormat = PhoneFormat.RussiaOnlyHyphenated;

			this.TextInserted += PhoneTextInserted;
			this.TextDeleted += PhoneTextDeleted;
		}

		protected void PhoneTextInserted (object o, Gtk.TextInsertedArgs args)
		{
			var curPos = args.Position;
			var formated = phoneFormatter.FormatString (Text, ref curPos);
			var OldTextLegth = Text.Length;
			if(Text != formated)
			{
				Text = formated;
				args.Position = curPos;
			}
		}

		protected void PhoneTextDeleted (object o, Gtk.TextDeletedArgs args)
		{
			if (args.EndPos == -1)
				return;

			int curPos = args.StartPos;
			var formated = phoneFormatter.FormatString(Text, ref curPos);

			if (Text != formated)
			{
				Text = formated;
				Position = curPos;
			}
		}

		protected override void OnChanged()
		{
			Binding.FireChange(w => w.Text);
			base.OnChanged();
		}
	}
}


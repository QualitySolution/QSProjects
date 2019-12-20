using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Gamma.Binding.Core;

namespace QS.Widgets
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class ValidatedEntry : Gtk.Entry
	{
		public BindingControler<ValidatedEntry> Binding { get; private set; }

		private Regex regex;
		private ValidationType validationMode;
		public ValidationType ValidationMode {
			get {
				return validationMode;
			}
			set { 
				validationMode = value;
				switch(validationMode) {
					case ValidationType.Numeric:
						this.TextInserted += NumericValidate;
						break;
					case ValidationType.Price:
						regex = new Regex(@"^[0-9]{1,6}(\.[0-9]{1,2})?$");
						this.Changed += RegexValidate;
						break;
					case ValidationType.Email:
						regex = new Regex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@" +
							@"[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]\.[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]$");
						this.Changed += RemoveInvalidSymbols;
						this.Changed += RegexValidate;
						break;
					default:
						break;
				}
			}
		}

		/// <summary>
		/// Для хранения пользовательской информации как в WinForms
		/// </summary>
		public object Tag;

		public ValidatedEntry () 
		{
			ValidationMode = ValidationType.None;
			Binding = new BindingControler<ValidatedEntry>(this, new Expression<Func<ValidatedEntry, object>>[] {
				(w => w.Text)
			});
		}

		protected void NumericValidate(object sender, Gtk.TextInsertedArgs Args)
		{
			String Text = (sender as Gtk.Entry).Text;
			Text = Regex.Replace (Text, "[^0-9]", "");
			(sender as Gtk.Entry).Text = Text;
		}

		protected void RegexValidate(object sender, System.EventArgs Args)
		{
			String Text = (sender as Gtk.Entry).Text;
			if (!regex.IsMatch(Text))
				(sender as Gtk.Entry).ModifyText(Gtk.StateType.Normal, new Gdk.Color(255,0,0));
			else
				(sender as Gtk.Entry).ModifyText(Gtk.StateType.Normal);
		}

		protected void RemoveInvalidSymbols(object sender, System.EventArgs Args)
		{
			this.Text = Text.Replace(" ", "").Replace("\n", "");
		}

		protected override void OnChanged()
		{
			Binding.FireChange(w => w.Text);
			base.OnChanged();
		}
	}

	public enum ValidationType {
		None,
		Numeric,
		Email,
		Price
	};
}


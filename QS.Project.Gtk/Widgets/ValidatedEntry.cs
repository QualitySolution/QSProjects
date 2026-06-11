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
		private string customRegexPattern;

		public string CustomRegex {
			get => customRegexPattern;
			set {
				customRegexPattern = value;
				if(!string.IsNullOrEmpty(value)) {
					regex = new Regex(value);
					validationMode = ValidationType.CustomRegex;
					this.Changed += RegexValidate;
				}
			}
		}
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
							@"([a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]\.)+[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]$");
						this.Changed += RemoveInvalidSymbols;
						this.Changed += RegexValidate;
						break;
					case ValidationType.MultipleEmail:
						regex = new Regex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@" +
							@"[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]\.[a-zA-Z0-9][a-zA-Z0-9-]{0,61}[a-zA-Z0-9]$");
						this.Changed += RemoveInvalidSymbolsMultiple;
						this.Changed += MultipleEmailValidate;
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

		protected void RemoveInvalidSymbolsMultiple(object sender, System.EventArgs Args)
		{
			this.Text = Text.Replace(" ", "").Replace("\n", "").Replace(";", ",");
		}

		protected void MultipleEmailValidate(object sender, System.EventArgs Args)
		{
			var text = (sender as Gtk.Entry).Text;
			if(string.IsNullOrEmpty(text)) {
				(sender as Gtk.Entry).ModifyText(Gtk.StateType.Normal);
				return;
			}
			var emails = text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			bool allValid = emails.Length > 0 && System.Array.TrueForAll(emails, e => regex.IsMatch(e.Trim()));
			if(!allValid)
				(sender as Gtk.Entry).ModifyText(Gtk.StateType.Normal, new Gdk.Color(255, 0, 0));
			else
				(sender as Gtk.Entry).ModifyText(Gtk.StateType.Normal);
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
		MultipleEmail,
		Price,
		CustomRegex
	};
}


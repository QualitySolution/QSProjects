using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Gamma.Binding.Core;
using Gdk;
using Gtk;

namespace QS.Widgets.GtkUI
{
	[ToolboxItem(true)]
	[Category("QS.Project")]
	public partial class VehicleRegistrationNumberEntry : Bin
	{
		private string _number;
		private Country _country;

		private readonly Color _colorBlack;
		private static readonly Color _colorRed = new Color(255, 0, 0);
		private static readonly Color _colorDarkRed = new Color(210, 0, 0);

		public VehicleRegistrationNumberEntry()
		{
			Build();

			_colorBlack = Rc.GetStyle(entryNumber).Foreground(StateType.Normal);

			Binding = new BindingControler<VehicleRegistrationNumberEntry>(this,
				new Expression<Func<VehicleRegistrationNumberEntry, object>>[] { w => w.Number });

			comboCountry.ItemsEnum = typeof(Country);
			comboCountry.Changed += (sender, args) => _country = (Country)comboCountry.SelectedItem;
			comboCountry.SelectedItem = Country;

			entryNumber.Changed += (sender, args) => UpdateNumber(entryNumber.Text, false);
		}

		public BindingControler<VehicleRegistrationNumberEntry> Binding { get; private set; }

		public string Text => entryNumber.Text;

		public string Number
		{
			get => _number;
			set => UpdateNumber(value, false);
		}

		public Country Country
		{
			get => _country;
			set
			{
				if(value == _country)
				{
					return;
				}
				_country = value;
				comboCountry.SelectedItem = _country;
				UpdateNumber(_number, true);
			}
		}

		private void UpdateNumber(string value, bool forceUpdate)
		{
			if(value != null)
			{
				value = Parse(value);
			}

			if(value == _number && !forceUpdate)
			{
				return;
			}

			var valid = Validate(value);
			_number = valid ? value : null;
			entryNumber.ModifyText(StateType.Normal, valid ? _colorBlack : _colorRed);
			entryNumber.ModifyText(StateType.Insensitive, valid ? _colorBlack : _colorDarkRed);
			entryNumber.Text = value;
			Binding.FireChange(w => w.Number);
		}

		private bool Validate(string text)
		{
			switch(_country)
			{
				case Country.Ru:
					return Regex.IsMatch(text ?? "", @"^[ABEKMHOPCTYX]\d{3}[ABEKMHOPCTYX]{2}\d{2,3}$");
				default:
					throw new NotSupportedException($"Validation for country '{_country}' is not supported");
			}
		}

		private string Parse(string input)
		{
			switch(_country)
			{
				case Country.Ru:
					return ConvertAllLettersFromRuToEn(input.ToUpper());
				default:
					return input;
			}
		}

		private static string ConvertAllLettersFromRuToEn(string inputText)
		{
			var charText = inputText.ToCharArray();
			for(int i = 0; i < charText.Length; i++)
			{
				if(RuToEnLetters.TryGetValue(inputText[i], out var convertedValue))
				{
					charText[i] = convertedValue;
				}
			}
			return new string(charText);
		}

		public static readonly IDictionary<char, char> RuToEnLetters = new Dictionary<char, char>
		{
			{ 'А', 'A' },
			{ 'В', 'B' },
			{ 'Е', 'E' },
			{ 'К', 'K' },
			{ 'М', 'M' },
			{ 'Н', 'H' },
			{ 'О', 'O' },
			{ 'Р', 'P' },
			{ 'С', 'C' },
			{ 'Т', 'T' },
			{ 'У', 'Y' },
			{ 'Х', 'X' }
		};
	}

	public enum Country
	{
		Ru
	}
}

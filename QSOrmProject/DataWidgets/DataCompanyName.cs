using System;
using System.Collections.Generic;
using Gtk;
using NLog;

namespace Gtk.DataBindings
{
	[System.ComponentModel.ToolboxItem(true)]
	public class DataCompanyName : DataEntry
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public Entry FullNameEntry;

		private bool ParalelEditing;

		private Dictionary<string, string> Replaces;

		public DataCompanyName () : base()
		{
			Replaces = new Dictionary<string, string>() {
				{"ООО", "Общество с ограниченной ответственностью"},
				{"ЗАО", "Закрытое акционерное общество"},
				{"ОАО", "Открытое акционерное общество"},
				{"ГорПО", "Городское потребительское общество"},
				{"РайПО", "Районное потребительское общество"},
				{"СельПО", "Сельское потребительское общество"},
				{"НКО", "Некоммерческая организация"},
				{"ИП", "Индивидуальный предприниматель"},
				{"ОДО", "Общество с дополнительной ответственностью"},
				{"ПТ", "Полное товарищество"},
				{"АО", "Акционерное общество"},
				{"КТ", "Коммандитное товарищество"},
				{"ХП", "Хозяйственное партнерство"},
				{"КФХ", "Крестьянское (фермерское) хозяйство"},
				{"НП", "Некоммерческое партнерство"},
				{"ПК", "Потребительский кооператив"},
				{"ТСН", "Товарищество собственников недвижимости"},
				{"АНО", "Автономная некоммерческая организация"},
				{"ФГУП", "Федеральное государственное унитарное предприятие"},
				{"ГУП", "Государственное унитарное предприятие"},
				{"МУП", "Муниципальное унитарное предприятие"},
				{"ГП", "Государственное предприятие"},
				{"СНТ", "Садоводческое некоммерческое товарищество"},
				{"ДНТ", "Дачное некоммерческое товарищество"},
				{"ОНТ", "Огородническое некоммерческое товарищество"},
				{"СНП", "Садоводческое некоммерческое партнерство"},
				{"ДНП", "Дачное некоммерческое партнерство"},
				{"ОНП", "Огородническое некоммерческое партнерство"},
				{"СПК", "Садоводческий потребительский кооператив"},
				{"ДПК", "Дачный потребительский кооператив"},
				{"ОПК", "Огороднический потребительский кооператив"}
			};
		}

		protected override void OnStateChanged (StateType previous_state)
		{
			Console.WriteLine (State.ToString ());
			base.OnStateChanged (previous_state);
		}

		protected override bool OnFocusInEvent (Gdk.EventFocus evnt)
		{
			logger.Debug ("Focused");
			ParalelEditing = false;
			if(FullNameEntry != null)
			{
				if (Text == FullNameEntry.Text)
					ParalelEditing = true;
				if(FullNameEntry.Text == String.Empty)
					ParalelEditing = true;
				if(FindQuotedText (Text) == FindQuotedText(FullNameEntry.Text))
					ParalelEditing = true;

				if(ParalelEditing)
				{
					logger.Debug ("Включен режим паралельного редактирования.");
					FullNameEntry.ModifyText (StateType.Normal, new Gdk.Color(0, 152, 190));
				}
			}
			return base.OnFocusInEvent (evnt);
		}

		protected override bool OnFocusOutEvent (Gdk.EventFocus evnt)
		{
			if(ParalelEditing)
				FullNameEntry.ModifyText (StateType.Normal);
			return base.OnFocusOutEvent (evnt);
		}

		private string FindQuotedText(string inputStr)
		{
			int beginQuote = inputStr.IndexOfAny (new char[]{
				(char) 0034,
				(char) 0171,
				(char) 8222,
				(char) 8223
			});
			int endOuoted = inputStr.LastIndexOfAny (new char[]{
				(char) 0034,
				(char) 0187,
				(char) 8220,
				(char) 8221
			});
			if (beginQuote < 0 || endOuoted < 0)
				return String.Empty;
			if(beginQuote == endOuoted)
				return String.Empty;
			return inputStr.Substring (beginQuote + 1, endOuoted - beginQuote - 1);
		}

		protected override void OnChanged ()
		{
			if(ParalelEditing)
			{
				string ModifedText = Text;
				foreach(KeyValuePair<string, string> pair in Replaces)
				{
					ModifedText = ModifedText.Replace (pair.Key, pair.Value);
				}
				FullNameEntry.Text = ModifedText;
			}
			base.OnChanged ();
		}
	}
}


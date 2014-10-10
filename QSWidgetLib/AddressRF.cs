using System;
using Gtk;

namespace QSWidgetLib
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AddressRF : Gtk.Bin
	{
		ListStore CityStore = new ListStore (typeof (string));


		public AddressRF()
		{
			this.Build();

			entryCity.Completion = new Gtk.EntryCompletion();
			entryCity.Completion.PrefixInserted += CityPrefixInserted;
			entryCity.Completion.Model = CityStore;
			entryCity.Completion.TextColumn = 0;

			CityStore.AppendValues("Санкт-Петербург");
			CityStore.AppendValues("Москва");
			CityStore.AppendValues("Отрадное");
		}

		void CityPrefixInserted (object o, Gtk.PrefixInsertedArgs args)
		{
			Console.WriteLine(args.Prefix);
		}
	}
}


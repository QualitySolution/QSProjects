using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using Gamma.Binding.Core;
using Gtk;
using QSOsm.Loaders;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma OSM Widgets")]
	public class StreetEntry : Entry
	{
		enum columns
		{
			Street,
			District
		}

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


		private IStreetsDataLoader streetsDataLoader;
		public IStreetsDataLoader StreetsDataLoader {
			get { return streetsDataLoader; }
			set { ChangeDataLoader(streetsDataLoader, value); }
		}

		private ListStore completionListStore;

		public event EventHandler StreetSelected;

		public BindingControler<StreetEntry> Binding { get; private set; }

		long cityId;

		public long CityId {
			get { return cityId; }
			set {
				if(cityId == value)
					return;
				cityId = value;
			}
		}

		public bool ExistOnOSM { get; private set; }

		string street;

		public string Street {
			get {
				return street;
			}
			set {
				street = value;
				UpdateText();
			}
		}

		string streetDistrict;

		public string StreetDistrict {
			get {
				return streetDistrict;
			}
			set {
				streetDistrict = value;
				UpdateText();
			}
		}

		void UpdateText()
		{
			this.Text = GenerateEntryText();
			OnStreetSelected();
		}

		string GenerateEntryText()
		{
			return String.IsNullOrWhiteSpace(StreetDistrict) ?
								Street
								: String.Format($"{Street} ({StreetDistrict})");
		}

		public StreetEntry()
		{
			Binding = new BindingControler<StreetEntry>(this, new Expression<Func<StreetEntry, object>>[] {
				(w => w.Street), (w => w.StreetDistrict)
			});

			this.Completion = new EntryCompletion();
			this.Completion.MinimumKeyLength = 0;
			this.Completion.MatchSelected += Completion_MatchSelected;
			this.Completion.MatchFunc = (completion, key, iter) => true;
			var cell = new CellRendererText();
			this.Completion.PackStart(cell, true);
			this.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);
		}

		//Костыль, для отображения выпадающего списка
		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if(evnt.Key == Gdk.Key.Control_R)
				this.InsertText("");

			return base.OnKeyPressEvent(evnt);
		}

		bool firstCompletion = true;

		private void EntryTextChanges(object o, TextInsertedArgs args)
		{
			if(cityId != 0) {
				if(firstCompletion) {
					EmptyCompletion();
					firstCompletion = false;
				}
				streetsDataLoader.LoadStreets(cityId, Text);
			}
		}

		private void EntryTextChanges(object o, TextDeletedArgs args) => EntryTextChanges(o, TextInsertedArgs.Empty as TextInsertedArgs);


		void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var streetName = (string)tree_model.GetValue(iter, (int)columns.Street);
			var district = (string)tree_model.GetValue(iter, (int)columns.District);
			string pattern = String.Format("\\b{0}", Regex.Escape(Text.ToLower()));
			streetName = Regex.Replace(streetName, pattern, (match) => String.Format("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			(cell as CellRendererText).Markup = String.IsNullOrWhiteSpace(district) ?
				streetName :
				String.Format("{0} ({1})", streetName, district);
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected(object o, MatchSelectedArgs args)
		{
			if(args.Model.GetValue(args.Iter, (int)columns.Street).ToString() == "Загрузка...") {
				args.RetVal = false;
				return;
			}
			Street = args.Model.GetValue(args.Iter, (int)columns.Street).ToString();
			StreetDistrict = args.Model.GetValue(args.Iter, (int)columns.District).ToString();
			ExistOnOSM = true;
			args.RetVal = true;
		}

		protected override bool OnFocusOutEvent(Gdk.EventFocus evnt)
		{
			if(Text != GenerateEntryText()) {
				var userText = Text;
				ExistOnOSM = false;
				StreetDistrict = String.Empty;
				Street = userText;
			}
			return base.OnFocusOutEvent(evnt);
		}

		private void ChangeDataLoader(IStreetsDataLoader oldValue, IStreetsDataLoader newValue)
		{
			if(oldValue == newValue)
				return;
			if(oldValue != null) {
				oldValue.StreetLoaded -= StreetLoaded;
				this.TextInserted -= EntryTextChanges;
				this.TextDeleted -= EntryTextChanges;
			}

			streetsDataLoader = newValue;
			if(StreetsDataLoader == null)
				return;
			StreetsDataLoader.StreetLoaded += StreetLoaded;
			this.TextInserted += EntryTextChanges;
			this.TextDeleted += EntryTextChanges;
		}

		private void EmptyCompletion()
		{
			completionListStore = new ListStore(typeof(string), typeof(string));
			completionListStore.AppendValues("Загрузка...", "");
			this.Completion.Model = completionListStore;
			this.Completion.Complete();
		}

		private void StreetLoaded()
		{

				var streets = streetsDataLoader.GetStreets();
				completionListStore = new ListStore(typeof(string), typeof(string));
				foreach(var s in streets) {
					completionListStore.AppendValues(
						s.Name,
						s.Districts
					);
				}
			Application.Invoke((sender, e) => {
				this.Completion.Model = completionListStore;
				if(this.HasFocus) {
					this.Completion.Complete();
				}
			});
		}

		protected override void OnChanged()
		{
			Binding.FireChange(w => w.Street, w => w.StreetDistrict);
			base.OnChanged();
		}

		protected virtual void OnStreetSelected()
		{
			StreetSelected?.Invoke(null, EventArgs.Empty);
		}

		protected override void OnDestroyed()
		{
			if(StreetsDataLoader != null)
				StreetsDataLoader.StreetLoaded -= StreetLoaded;
			base.OnDestroyed();
		}
	}
}


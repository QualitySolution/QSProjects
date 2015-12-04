﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using Gamma.Binding.Core;
using Gtk;
using QSOsm.DTO;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public class HouseEntry : Entry
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public event EventHandler CompletionLoaded;

		private ListStore completionListStore;

		private Thread queryThread;

		public BindingControler<HouseEntry> Binding { get; private set; }

		public bool? OsmCompletion { 
			get {
				if (completionListStore == null)
					return null;
				return completionListStore.Cast<object[]> ().Any (row => (string)row [0] == House);
			}
		}

		public string House {
			get {
				return Text;
			}
			set {
				if (Text == value)
					return;
				this.Text = value;
			}
		}

		OsmStreet street;

		public OsmStreet Street {
			get { 
				return street;
			}
			set {
				street = value;
				OnStreetSet ();
			}
		}

		public HouseEntry ()
		{
			Binding = new BindingControler<HouseEntry> (this, new Expression<Func<HouseEntry, object>>[] {
				(w => w.House),
				(w => w.OsmCompletion),
				(w => w.Text)
			});

			this.Completion = new EntryCompletion ();
			this.Completion.MinimumKeyLength = 0;
			this.Completion.MatchSelected += Completion_MatchSelected;
			this.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText ();
			this.Completion.PackStart (cell, true);
			this.Completion.SetCellDataFunc (cell, OnCellLayoutDataFunc);
		}

		void OnCellLayoutDataFunc (CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var houseNumber = (string)tree_model.GetValue (iter, 0);
			string pattern = String.Format ("{0}", Regex.Escape (Text.ToLower ()));
			houseNumber = Regex.Replace (houseNumber, pattern, (match) => String.Format ("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			(cell as CellRendererText).Markup = houseNumber;
		}


		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			var val = (string)completion.Model.GetValue (iter, 0);
			return Regex.IsMatch (val, String.Format ("{0}", Regex.Escape (this.Text)), RegexOptions.IgnoreCase);
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			House = args.Model.GetValue (args.Iter, 0).ToString ();
			args.RetVal = true;
		}


		void OnStreetSet ()
		{
			if (queryThread != null && queryThread.IsAlive) {
				try {
					queryThread.Abort ();
				} catch (ThreadAbortException ex) {
					logger.Warn ("fillAutocomplete() thread for houses was aborted.");
				}
			}
			queryThread = new Thread (fillAutocomplete);
			queryThread.IsBackground = true;
			queryThread.Start ();
		}

		private void fillAutocomplete ()
		{
			if (String.IsNullOrWhiteSpace (Street.Name)) {
				if (completionListStore != null)
					completionListStore.Clear ();				
			} else {
				
				logger.Info ("Запрос домов на {0}...", Street.Name);
				IOsmService svc = OsmWorker.GetOsmService ();

				List<string> houses;
				if (Street.Districts == null)
					houses = svc.GetHouseNumbersWithoutDistrict (Street.CityId, Street.Name);
				else
					houses = svc.GetHouseNumbers (Street.CityId, Street.Name, Street.Districts);
				completionListStore = new ListStore (typeof(string));
				foreach (var h in houses) {
					completionListStore.AppendValues (h);
				}
				this.Completion.Model = completionListStore;
				logger.Debug ("Получено {0} домов...", houses.Count);
			}
			if (CompletionLoaded != null)
				Gtk.Application.Invoke(CompletionLoaded);
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.House, w => w.Text, w => w.OsmCompletion);
			base.OnChanged ();
		}
	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using Gtk.DataBindings;
using System.Collections;
using System.Reflection;

namespace QSOrmProject.RepresentationModel
{
	public abstract class RepresentationModelBase<TNode>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region IRepresentationModel implementation

		IUnitOfWork uow;

		public IUnitOfWork UoW {
			get { return uow; }
			set { uow = value; }
		}

		public event EventHandler ItemsListUpdated;

		protected void OnItemsListUpdated()
		{
			if (ItemsListUpdated != null)
				ItemsListUpdated (this, EventArgs.Empty);
		}

		List<TNode> filtredItemsList;

		IList<TNode> itemsList;

		public IList ItemsList {
			get {
				return filtredItemsList ?? itemsList as IList;
			}
		}

		protected void SetItemsSource (IList<TNode> list)
		{
			itemsList = list;

			if (String.IsNullOrWhiteSpace (SearchString))
				OnItemsListUpdated ();
			else
				OnSearchRefilter ();
		}

		public virtual IMappingConfig TreeViewConfig { get{ throw new NotImplementedException ();
			} }

		private IRepresentationFilter representationFilter;

		public IRepresentationFilter RepresentationFilter {
			get { if (representationFilter != null)
					return representationFilter;
				if (CreateRepresentationFilter != null)
					RepresentationFilter = CreateRepresentationFilter();
				return representationFilter;
			}
			protected set { 
				if (representationFilter != null)
					representationFilter.Refiltered -= RepresentationFilter_Refiltered;
				representationFilter = value;
				if (representationFilter != null)
					representationFilter.Refiltered += RepresentationFilter_Refiltered;
			}
		}

		/// <summary>
		/// Функция создания нового фильтра необходима для отложенной загрузки.
		/// </summary>
		/// <value>The create representation filter.</value>
		public Func<IRepresentationFilter> CreateRepresentationFilter { get; set;}

		public IEnumerable<string> SearchFields {
			get {
				foreach (var prop in typeof(TNode).GetProperties ()) {
					var att = prop.GetCustomAttributes (typeof(UseForSearchAttribute), true).FirstOrDefault ();
					if (att != null)
						yield return prop.Name;
				}
			}
		}

		void RepresentationFilter_Refiltered (object sender, EventArgs e)
		{
			UpdateNodes ();
		}

		public abstract void UpdateNodes ();

		public Type NodeType {
			get { return typeof(TNode); }
		}

		string searchString;

		public string SearchString {
			get{
				return searchString;
			}
			set{
				if (searchString == value)
					return;

				searchString = value;
				OnSearchRefilter ();
			}
		}

		#endregion

		private PropertyInfo[] searchPropCache;

		protected PropertyInfo[] SearchPropCache {
			get {
				if (searchPropCache != null)
					return searchPropCache;

				searchPropCache = typeof(TNode).GetProperties ()
					.Where (prop => prop.GetCustomAttributes (typeof(UseForSearchAttribute), true).Length > 0)
					.ToArray ();

				return searchPropCache;
			}
		}

		protected void OnSearchRefilter()
		{
			if (String.IsNullOrWhiteSpace (SearchString))
				filtredItemsList = null;
			else
				filtredItemsList = itemsList.Where (SearchFilterFunc).ToList ();
			
			OnItemsListUpdated ();
		}

		private bool SearchFilterFunc(TNode item)
		{
			foreach (var prop in SearchPropCache) {
				string Str = (prop.GetValue (item, null) ?? String.Empty).ToString ();
				if (Str.IndexOf (searchString, StringComparison.CurrentCultureIgnoreCase) > -1) {
					return true;
				}
			}
			return false;
		}
	}
}


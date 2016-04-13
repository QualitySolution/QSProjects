using System;
using Gtk;

namespace QSTDI
{
	public class TdiTabBase : Bin, ITdiTab
	{
		public TdiTabBase ()
		{
		}

		#region ITdiTab implementation

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		private string tabName = String.Empty;

		public string TabName {
			get { return tabName;
			}
			protected set {
				if (tabName == value)
					return;
				tabName = value;
				OnTabNameChanged ();
			}
		}

		public ITdiTabParent TabParent { set; get; }

		public bool FailInitialize { get; protected set;}

		public virtual bool CompareHashName(string hashName)
		{
			return GenerateHashName(this.GetType()) == hashName;
		}

		public static string GenerateHashName<TTab>() where TTab : TdiTabBase
		{
			return GenerateHashName(typeof(TTab));
		}

		public static string GenerateHashName(Type tabType)
		{
			if (!typeof(TdiTabBase).IsAssignableFrom(tabType))
				throw new ArgumentException("Тип должен наследоваться от TdiTabBase", "tabType");

			return String.Format("Dlg_{0}", tabType.Name);
		}

		#endregion

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
		}

		protected void OnTabNameChanged()
		{
			if (TabNameChanged != null)
				TabNameChanged (this, new TdiTabNameChangedEventArgs (TabName));
		}

		protected void OpenNewTab(ITdiTab tab)
		{
			TabParent.AddTab (tab, this);
		}
	}
}


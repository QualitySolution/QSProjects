using System;
using NLog;
using NHibernate;
using System.Data.Bindings;
using QSOrmProject;
using QSTDI;
using System.Collections.Generic;

namespace QSProxies
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProxyDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private IProxyOwner proxyOwner;
		private Adaptor adaptor = new Adaptor();
		private Proxy subject;

		OrmParentReference parentReference;
		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if(!(parentReference.ParentObject is IProxyOwner))
					{
						throw new ArgumentException (String.Format("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IProxyOwner)));
					}
					this.proxyOwner = (IProxyOwner)parentReference.ParentObject;
				}
			}
			get {
				return parentReference;
			}
		}

		public ProxyDlg(OrmParentReference parentReference)
		{
			this.Build();
			ParentReference = parentReference;
			subject = new Proxy();
			proxyOwner.Proxies.Add (subject);
			ConfigureDlg();
		}

		public ProxyDlg(OrmParentReference parenReferance, Proxy subject)
		{
			this.Build();
			ParentReference = parenReferance;
			this.subject = subject;
			TabName = subject.Number;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entryNumber.IsEditable = true;
			adaptor.Target = subject;
			datatable1.DataSource = adaptor;
		}

		#region ITdiTab implementation
		public event EventHandler<QSTDI.TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<QSTDI.TdiTabCloseEventArgs> CloseTab;

		private string _tabName = "Новая доверенность";
		public string TabName
		{
			get{return _tabName;}
			set{
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged(this, new TdiTabNameChangedEventArgs(value));
			}

		}

		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		#endregion

		#region ITdiDialog implementation

		public bool Save ()
		{
			logger.Info("Сохраняем доверенность...");
			if(proxyOwner != null)
				OrmMain.DelayedNotifyObjectUpdated (proxyOwner, subject);
			return true;
		}

		public bool HasChanges {
			get {return Session.IsDirty();}
		}

		#endregion

		#region IOrmDialog implementation

		public NHibernate.ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set {
				session = value;
			}
		}

		public object Subject {
			get {return subject;}
			set {
				if (value is Proxy)
					subject = value as Proxy;
			}
		}
		#endregion

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save())
				OnCloseTab(false);
		}
			
		protected void OnCloseTab(bool askSave)
		{
			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
		}
	}
}


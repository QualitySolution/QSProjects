using System;
using Gtk;

namespace QSWidgetLib
{
	public class ParallelEditing
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public Entry OutEntry;
		public Func<object, string> GetParallelTextFunc;

		private bool IsParallelMode = false;

		public ParallelEditing (Entry outEntry)
		{
			OutEntry = outEntry;
		}

		public void SubscribeOnChanges(Entry sourceEntry)
		{
			sourceEntry.FocusInEvent += OnSourceFocusInEvent;
			sourceEntry.FocusOutEvent += OnSourceFocusOutEvent;
			sourceEntry.Changed += OnSourceChanged;
		}

		void OnSourceChanged (object sender, EventArgs e)
		{
			if(IsParallelMode)
				OutEntry.Text = GetParallelTextFunc(sender);
		}

		void OnSourceFocusOutEvent (object o, FocusOutEventArgs args)
		{
			if (IsParallelMode) {
				OutEntry.ModifyText (StateType.Normal);
				IsParallelMode = false;
			}
		}

		void OnSourceFocusInEvent (object o, FocusInEventArgs args)
		{
			IsParallelMode = false;
			if(OutEntry != null && GetParallelTextFunc != null)
			{
				if (String.IsNullOrEmpty (OutEntry.Text))
					IsParallelMode = true;
				else
					IsParallelMode = OutEntry.Text == GetParallelTextFunc(o);
			}

			if(IsParallelMode)
			{
				logger.Debug ("Включен режим паралельного редактирования.");
				OutEntry.ModifyText (StateType.Normal, new Gdk.Color(0, 152, 190));
			}
		}
	}
}


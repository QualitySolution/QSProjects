﻿using System;
using Gtk;
using QS.Journal.GtkUI;

namespace QS.Tdi
{
	public class DefaultTDIWidgetResolver : ITDIWidgetResolver
	{
		public virtual Widget Resolve(ITdiTab tab) => tab as Widget;
	}
}

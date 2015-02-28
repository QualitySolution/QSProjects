using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings;
using System.Reflection;
using Gtk;
using NLog;
using QSWidgetLib;

namespace QSOrmProject
{

	public enum SpecialComboState {
		[ItemTitle("Ничего")]
		None,
		[ItemTitle("Все")]
		All,
		[ItemTitle("Нет")]
		Not
	}
}

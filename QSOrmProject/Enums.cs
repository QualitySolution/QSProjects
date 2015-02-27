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
		[ItemTitleAttribute("Ничего")]
		None,
		[ItemTitleAttribute("Все")]
		All,
		[ItemTitleAttribute("Нет")]
		Not
	}
}

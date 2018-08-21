using System;
using System.Collections.Generic;
using Gtk;

namespace QS.Print
{
	public interface IOdtDocPrinter
	{
		void Print(IPrintableDocument[] documents, PrintSettings printSettings = null);
	}
}

using System;

namespace QS.Tdi
{
	public interface ITdiTabWithPath
	{
		string[] PathNames { get;}
		event EventHandler PathChanged;
	}
}
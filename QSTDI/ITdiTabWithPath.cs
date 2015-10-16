using System;

namespace QSTDI
{
	public interface ITdiTabWithPath
	{
		string[] PathNames { get;}
		event EventHandler PathChanged;
	}
}
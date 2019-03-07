using System;
using System.Threading;

namespace QS.Project.Dialogs
{
	public interface IRunOperationView
	{
		bool RunOperation(ThreadStart function, int timeout, string text);
	}
}

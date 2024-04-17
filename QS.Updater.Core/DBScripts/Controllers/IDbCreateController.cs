﻿using System;
using QS.Dialog;

namespace QS.DBScripts.Controllers
{
	public interface IDbCreateController
	{
		IProgressBarDisplayable Progress { get; }

		void WasError(String text, String lastExecutedStatement);

		bool NeedDropDatabaseIfExists(string dbname);
	}
}

using System;

namespace QS.ErrorReporting {
	public interface IErrorHandlingService {

		/// <param name="operationTitle">что именно не получилось</param>
		void Handle(Exception exception, string operationTitle = null);
	}
}

using System.Collections.Generic;

namespace QS.Report {
	public interface IReportInfoFactory {
		ReportInfo Create();
		ReportInfo Create(string identifier, string title, Dictionary<string, object> parameters);
	}
}

using QS.Project.VersionControl;

namespace QS.ErrorReporting
{
	public interface IErrorReporter
	{
		IDataBaseInfo DatabaseInfo { get; }
		IApplicationInfo ApplicationInfo { get; }

		bool SendErrorReport(IErrorReportingSettings settings);
	}
}

using System;
namespace QS.Project.VersionControl
{
	public interface IApplicationInfo
	{
		string ProductName { get; }
		string Edition { get; }
		Version Version { get; }
		string SerialNumber { get; }
		string DBName { get; }
	}
}
